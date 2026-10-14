using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where ForwardTimesheetWorkflowsStatus hands each record.</summary>
internal interface IForwardTimesheetWorkflowsStatusTarget
{
    Task ApplyAsync(ForwardTimesheetWorkflowsStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardTimesheetWorkflowsStatus keeps of each ListTimesheetWorkflows response.</summary>
internal sealed class ForwardTimesheetWorkflowsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Sends records to acme.employees.v1.EmployeeService.</summary>
internal sealed class ForwardTimesheetWorkflowsStatusEmployeeTarget(ApiEmployees.EmployeeService.EmployeeServiceClient client) : IForwardTimesheetWorkflowsStatusTarget
{
    public async Task ApplyAsync(ForwardTimesheetWorkflowsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.CloneEmployeeRequest { RequestId = record.Status, Employee = new ApiEmployees.Employee { EmployeeId = record.NextPageToken } }, token, client.CloneEmployeeAsync);
    }
}

/// <summary>Passes records to acme.workplaces.v1.DeskApprovalService.</summary>
internal sealed class ForwardTimesheetWorkflowsStatusDeskApprovalRelay(ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient client) : IForwardTimesheetWorkflowsStatusTarget
{
    public async Task ApplyAsync(ForwardTimesheetWorkflowsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.GetDeskApprovalStatusRequest { DeskId = record.NextPageToken }, token, client.GetDeskApprovalStatusAsync);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelPolicySyncService.</summary>
internal sealed class ForwardTimesheetWorkflowsStatusTravelPolicySyncTarget : IForwardTimesheetWorkflowsStatusTarget
{
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _client;

    public ForwardTimesheetWorkflowsStatusTravelPolicySyncTarget(ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient client) => _client = client;

    public async Task ApplyAsync(ForwardTimesheetWorkflowsStatusRecord record, CancellationToken token)
    {
        await _client.GetTravelPolicySyncStateAsync(new ApiTravel.GetTravelPolicySyncStateRequest { TravelPolicyId = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.onboarding.v1.EquipmentCalculationService.</summary>
internal sealed class ForwardTimesheetWorkflowsStatusEquipmentCalculationSink(ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient client) : IForwardTimesheetWorkflowsStatusTarget
{
    public async Task ApplyAsync(ForwardTimesheetWorkflowsStatusRecord record, CancellationToken token)
    {
        await client.ExplainEquipmentCalculationAsync(new ApiOnboarding.ExplainEquipmentCalculationRequest { EquipmentId = record.Status, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardTimesheetWorkflowsStatusLogOnly : IForwardTimesheetWorkflowsStatusTarget
{
    public Task ApplyAsync(ForwardTimesheetWorkflowsStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.TimesheetWorkflowService/ListTimesheetWorkflows and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardTimesheetWorkflowsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient(channel);
        var reply = await source.ListTimesheetWorkflowsAsync(new ApiTimekeeping.ListTimesheetWorkflowsRequest() { OrderBy = "current-forward-timesheet-workflows-status" }, cancellationToken: token);
        var record = new ForwardTimesheetWorkflowsStatusRecord
        {
            Status = reply.Result.Status,
            CorrelationId = reply.Result.CorrelationId,
            NextPageToken = reply.PageInfo.NextPageToken,
        };
        await DeliverForwardTimesheetWorkflowsStatusAsync("equipment-calculation", record, channel, token);
    }

    private static async Task DeliverForwardTimesheetWorkflowsStatusAsync(string kind, ForwardTimesheetWorkflowsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IForwardTimesheetWorkflowsStatusTarget sink = kind switch
        {
            "employee" => new ForwardTimesheetWorkflowsStatusEmployeeTarget(new ApiEmployees.EmployeeService.EmployeeServiceClient(channel)),
            "desk-approval" => new ForwardTimesheetWorkflowsStatusDeskApprovalRelay(new ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient(channel)),
            "travel-policy-sync" => new ForwardTimesheetWorkflowsStatusTravelPolicySyncTarget(new ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient(channel)),
            "equipment-calculation" => new ForwardTimesheetWorkflowsStatusEquipmentCalculationSink(new ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient(channel)),
            _ => new ForwardTimesheetWorkflowsStatusLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
