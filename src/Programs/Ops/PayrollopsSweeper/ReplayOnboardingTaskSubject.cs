using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ReplayOnboardingTaskSubject hands each record.</summary>
internal interface IReplayOnboardingTaskSubjectSink
{
    Task DeliverAsync(ReplayOnboardingTaskSubjectRecord record, CancellationToken token);
}

/// <summary>What ReplayOnboardingTaskSubject keeps of each PeekOnboardingTask response.</summary>
internal sealed class ReplayOnboardingTaskSubjectRecord
{
    public string Subject { get; init; } = "";
    public string OnboardingTaskId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.telehealth.v1.VideoVisitCalculationService.</summary>
internal sealed class ReplayOnboardingTaskSubjectVideoVisitCalculationWriter : IReplayOnboardingTaskSubjectSink
{
    private readonly ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient _client;

    public ReplayOnboardingTaskSubjectVideoVisitCalculationWriter(ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(ReplayOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTelehealth.RecalculateVideoVisitsRequest { VideoVisit = new ApiTelehealth.VideoVisit { VideoVisitId = record.Message }, VideoVisitId = record.Status }, token, _client.RecalculateVideoVisitsAsync);
    }
}

/// <summary>Sends records to acme.webhooks.v1.WebhookEndpointQueryService.</summary>
internal sealed class ReplayOnboardingTaskSubjectWebhookEndpointQuerySink(ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient client) : IReplayOnboardingTaskSubjectSink
{
    public async Task DeliverAsync(ReplayOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWebhooks.PeekWebhookEndpointRequest { WebhookEndpointId = record.Message }, token, client.PeekWebhookEndpointAsync);
    }
}

/// <summary>Sends records to acme.employees.v1.OrgUnitWorkflowService.</summary>
internal sealed class ReplayOnboardingTaskSubjectOrgUnitWorkflowWriter(ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient client) : IReplayOnboardingTaskSubjectSink
{
    public async Task DeliverAsync(ReplayOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        await client.PauseOrgUnitWorkflowAsync(new ApiEmployees.PauseOrgUnitWorkflowRequest { OrgUnitId = record.OnboardingTaskId, OrgUnit = new ApiEmployees.OrgUnit { Name = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.performance.v1.GoalWorkflowService.</summary>
internal sealed class ReplayOnboardingTaskSubjectGoalWorkflowTarget : IReplayOnboardingTaskSubjectSink
{
    private readonly ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient _client;

    public ReplayOnboardingTaskSubjectGoalWorkflowTarget(ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(ReplayOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        await _client.RetryGoalWorkflowAsync(new ApiPerformance.RetryGoalWorkflowRequest { GoalId = record.Status, Goal = new ApiPerformance.Goal { GoalId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskQueryService/PeekOnboardingTask and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayOnboardingTaskSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient(channel);
        var reply = await source.PeekOnboardingTaskAsync(new ApiOnboarding.PeekOnboardingTaskRequest() { OnboardingTaskId = "nightly-replay-onboarding-task-subject" }, cancellationToken: token);
        var record = new ReplayOnboardingTaskSubjectRecord
        {
            Subject = reply.OnboardingTask.Subject,
            OnboardingTaskId = Text.Normalize(reply.OnboardingTask.OnboardingTaskId),
            Message = reply.Result.Message,
            Status = reply.Result.Status,
        };
        var sinks = new Dictionary<string, IReplayOnboardingTaskSubjectSink>
        {
            ["video-visit-calculation"] = new ReplayOnboardingTaskSubjectVideoVisitCalculationWriter(new ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient(channel)),
            ["webhook-endpoint-query"] = new ReplayOnboardingTaskSubjectWebhookEndpointQuerySink(new ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient(channel)),
            ["org-unit-workflow"] = new ReplayOnboardingTaskSubjectOrgUnitWorkflowWriter(new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel)),
            ["goal-workflow"] = new ReplayOnboardingTaskSubjectGoalWorkflowTarget(new ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient(channel)),
        };
        await RouteReplayOnboardingTaskSubjectAsync("goal-workflow", record, sinks, token);
    }

    private static async Task RouteReplayOnboardingTaskSubjectAsync(string kind, ReplayOnboardingTaskSubjectRecord record, IReadOnlyDictionary<string, IReplayOnboardingTaskSubjectSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
