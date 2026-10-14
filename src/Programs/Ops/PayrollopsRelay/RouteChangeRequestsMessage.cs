using Grpc.Net.Client;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where RouteChangeRequestsMessage hands each record.</summary>
internal interface IRouteChangeRequestsMessageWriter
{
    Task SendAsync(RouteChangeRequestsMessageRecord record, CancellationToken token);
}

/// <summary>What RouteChangeRequestsMessage keeps of each SuggestChangeRequests response.</summary>
internal sealed class RouteChangeRequestsMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.employeeportal.v1.AnnouncementNotificationService.</summary>
internal sealed class RouteChangeRequestsMessageAnnouncementNotificationSink : IRouteChangeRequestsMessageWriter
{
    private readonly ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient _client;

    public RouteChangeRequestsMessageAnnouncementNotificationSink(ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient client) => _client = client;

    public async Task SendAsync(RouteChangeRequestsMessageRecord record, CancellationToken token)
    {
        await _client.NotifyAnnouncementWatchersAsync(new ApiEmployeeportal.NotifyAnnouncementWatchersRequest { Details = new ApiEmployeeportal.NotifyAnnouncementWatchersRequest.Types.Details { TemplateId = record.CorrelationId }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.employees.v1.OrgUnitWorkflowService.</summary>
internal sealed class RouteChangeRequestsMessageOrgUnitWorkflowSink(ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient client) : IRouteChangeRequestsMessageWriter
{
    public async Task SendAsync(RouteChangeRequestsMessageRecord record, CancellationToken token)
    {
        await client.CancelOrgUnitWorkflowAsync(new ApiEmployees.CancelOrgUnitWorkflowRequest { Details = new ApiEmployees.CancelOrgUnitWorkflowRequest.Types.Details { InitiatedBy = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.GoalWorkflowService.</summary>
internal sealed class RouteChangeRequestsMessageGoalWorkflowTarget : IRouteChangeRequestsMessageWriter
{
    private readonly ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient _client;

    public RouteChangeRequestsMessageGoalWorkflowTarget(ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(RouteChangeRequestsMessageRecord record, CancellationToken token)
    {
        await _client.PauseGoalWorkflowAsync(new ApiPerformance.PauseGoalWorkflowRequest { Reason = record.CorrelationId, Goal = new ApiPerformance.Goal { Subject = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteChangeRequestsMessageLogOnly : IRouteChangeRequestsMessageWriter
{
    public Task SendAsync(RouteChangeRequestsMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.itsm.v1.ChangeRequestQueryService/SuggestChangeRequests and hands what it keeps to the sink of its kind.</summary>
internal static class RouteChangeRequestsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient(channel);
        var got = await source.SuggestChangeRequestsAsync(new ApiItsm.SuggestChangeRequestsRequest() { OrderBy = "current-route-change-requests-message" }, cancellationToken: token);
        var record = new RouteChangeRequestsMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = got.Result.CorrelationId,
        };
        await DispatchRouteChangeRequestsMessageAsync("goal-workflow", record, channel, token);
    }

    private static async Task DispatchRouteChangeRequestsMessageAsync(string kind, RouteChangeRequestsMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<RouteChangeRequestsMessageRecord, CancellationToken, Task> write = new RouteChangeRequestsMessageLogOnly().SendAsync;
        switch (kind)
        {
            case "announcement-notification":
                write = new RouteChangeRequestsMessageAnnouncementNotificationSink(new ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient(channel)).SendAsync;
                break;
            case "org-unit-workflow":
                write = new RouteChangeRequestsMessageOrgUnitWorkflowSink(new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel)).SendAsync;
                break;
            case "goal-workflow":
                write = new RouteChangeRequestsMessageGoalWorkflowTarget(new ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient(channel)).SendAsync;
                break;
        }
        await write(record, token);
    }
}
