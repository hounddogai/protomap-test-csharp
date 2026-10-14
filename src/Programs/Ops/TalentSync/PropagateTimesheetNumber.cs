using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiFuel = Acme.Fuel.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where PropagateTimesheetNumber hands each record.</summary>
internal interface IPropagateTimesheetNumberSink
{
    Task DeliverAsync(PropagateTimesheetNumberRecord record, CancellationToken token);
}

/// <summary>What PropagateTimesheetNumber keeps of each GetTimesheet response.</summary>
internal sealed class PropagateTimesheetNumberRecord
{
    public string Number { get; init; } = "";
    public string TimesheetId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Sends records to acme.timekeeping.v1.OvertimeCalculationService.</summary>
internal sealed class PropagateTimesheetNumberOvertimeCalculationRelay(ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient client) : IPropagateTimesheetNumberSink
{
    public async Task DeliverAsync(PropagateTimesheetNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.SimulateOvertimeRequest { Details = new ApiTimekeeping.SimulateOvertimeRequest.Types.Details { CurrencyCode = record.TimesheetId }, OvertimeId = record.Notes, Overtime = new ApiTimekeeping.Overtime { Description = record.Status } }, token, client.SimulateOvertimeAsync);
    }
}

/// <summary>Hands records to acme.fuel.v1.FuelCardCalculationService.</summary>
internal sealed class PropagateTimesheetNumberFuelCardCalculationSink : IPropagateTimesheetNumberSink
{
    private readonly ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient _client;

    public PropagateTimesheetNumberFuelCardCalculationSink(ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(PropagateTimesheetNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFuel.EstimateFuelCardRequest { Reason = record.Notes, FuelCard = new ApiFuel.FuelCard { CurrencyCode = record.Status } }, token, _client.EstimateFuelCardAsync);
    }
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassApprovalService.</summary>
internal sealed class PropagateTimesheetNumberVisitorPassApprovalTarget(ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient client) : IPropagateTimesheetNumberSink
{
    public async Task DeliverAsync(PropagateTimesheetNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.CommentVisitorPassApprovalRequest { VisitorPass = new ApiWorkplaces.VisitorPass { Number = record.Status }, VisitorPassId = record.Number }, token, client.CommentVisitorPassApprovalAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateTimesheetNumberLogOnly : IPropagateTimesheetNumberSink
{
    public Task DeliverAsync(PropagateTimesheetNumberRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Number}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.TimesheetService/GetTimesheet and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateTimesheetNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.TimesheetService.TimesheetServiceClient(channel);
        var found = await source.GetTimesheetAsync(new ApiTimekeeping.GetTimesheetRequest() { TimesheetId = "default-propagate-timesheet-number" }, cancellationToken: token);
        var record = new PropagateTimesheetNumberRecord
        {
            Number = found.Timesheet.Number,
            TimesheetId = Text.Normalize(found.Timesheet.TimesheetId),
            Status = found.Result.Status,
            Notes = found.Timesheet.Notes,
        };
        await DispatchPropagateTimesheetNumberAsync("overtime-calculation", record, channel, token);
    }

    private static async Task DispatchPropagateTimesheetNumberAsync(string kind, PropagateTimesheetNumberRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<PropagateTimesheetNumberRecord, CancellationToken, Task> write = new PropagateTimesheetNumberLogOnly().DeliverAsync;
        switch (kind)
        {
            case "overtime-calculation":
                write = new PropagateTimesheetNumberOvertimeCalculationRelay(new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(channel)).DeliverAsync;
                break;
            case "fuel-card-calculation":
                write = new PropagateTimesheetNumberFuelCardCalculationSink(new ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient(channel)).DeliverAsync;
                break;
            case "visitor-pass-approval":
                write = new PropagateTimesheetNumberVisitorPassApprovalTarget(new ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
