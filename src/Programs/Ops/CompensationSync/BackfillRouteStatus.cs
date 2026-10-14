using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCompliance = Acme.Compliance.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiRouting = Acme.Routing.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where BackfillRouteStatus hands each record.</summary>
internal interface IBackfillRouteStatusDestination
{
    Task PushAsync(BackfillRouteStatusRecord record, CancellationToken token);
}

/// <summary>What BackfillRouteStatus keeps of each SimulateRoute response.</summary>
internal sealed class BackfillRouteStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Sends records to acme.timekeeping.v1.TimesheetWorkflowService.</summary>
internal sealed class BackfillRouteStatusTimesheetWorkflowTarget(ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient client) : IBackfillRouteStatusDestination
{
    public async Task PushAsync(BackfillRouteStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.StartTimesheetWorkflowRequest { Reason = record.Message, Timesheet = new ApiTimekeeping.Timesheet { Notes = record.Status } }, token, client.StartTimesheetWorkflowAsync);
    }
}

/// <summary>Passes records to acme.compliance.v1.ComplianceCaseQueryService.</summary>
internal sealed class BackfillRouteStatusComplianceCaseQueryWriter : IBackfillRouteStatusDestination
{
    private readonly ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient _client;

    public BackfillRouteStatusComplianceCaseQueryWriter(ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient client) => _client = client;

    public async Task PushAsync(BackfillRouteStatusRecord record, CancellationToken token)
    {
        await _client.PeekComplianceCaseAsync(new ApiCompliance.PeekComplianceCaseRequest { ComplianceCaseId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.employeeportal.v1.AnnouncementIntegrationService.</summary>
internal sealed class BackfillRouteStatusAnnouncementIntegrationTarget : IBackfillRouteStatusDestination
{
    private readonly ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient _client;

    public BackfillRouteStatusAnnouncementIntegrationTarget(ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient client) => _client = client;

    public async Task PushAsync(BackfillRouteStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployeeportal.ConnectAnnouncementSourceRequest { Details = new ApiEmployeeportal.ConnectAnnouncementSourceRequest.Types.Details { MessageId = record.Number, Endpoint = record.Message } }, token, _client.ConnectAnnouncementSourceAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillRouteStatusLogOnly : IBackfillRouteStatusDestination
{
    public Task PushAsync(BackfillRouteStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.routing.v1.RouteCalculationService/SimulateRoute and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillRouteStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRouting.RouteCalculationService.RouteCalculationServiceClient(channel);
        var response = await source.SimulateRouteAsync(new ApiRouting.SimulateRouteRequest() { Reason = "default-backfill-route-status" }, cancellationToken: token);
        var record = new BackfillRouteStatusRecord
        {
            Status = response.Result.Status,
            Message = response.Result.Message,
            Number = response.Route.Number,
        };
        await DeliverBackfillRouteStatusAsync("compliance-case-query", record, channel, token);
    }

    private static async Task DeliverBackfillRouteStatusAsync(string kind, BackfillRouteStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IBackfillRouteStatusDestination sink = new BackfillRouteStatusLogOnly();
        switch (kind)
        {
            case "timesheet-workflow":
                sink = new BackfillRouteStatusTimesheetWorkflowTarget(new ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient(channel));
                break;
            case "compliance-case-query":
                sink = new BackfillRouteStatusComplianceCaseQueryWriter(new ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient(channel));
                break;
            case "announcement-integration":
                sink = new BackfillRouteStatusAnnouncementIntegrationTarget(new ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient(channel));
                break;
        }
        await sink.PushAsync(record, token);
    }
}
