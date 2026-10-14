using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where SyncCorporateEventTotalsStatus hands each record.</summary>
internal interface ISyncCorporateEventTotalsStatusSink
{
    Task ForwardAsync(SyncCorporateEventTotalsStatusRecord record, CancellationToken token);
}

/// <summary>What SyncCorporateEventTotalsStatus keeps of each CountCorporateEventTotals response.</summary>
internal sealed class SyncCorporateEventTotalsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.performance.v1.GoalApprovalService.</summary>
internal sealed class SyncCorporateEventTotalsStatusGoalApprovalRelay : ISyncCorporateEventTotalsStatusSink
{
    private readonly ApiPerformance.GoalApprovalService.GoalApprovalServiceClient _client;

    public SyncCorporateEventTotalsStatusGoalApprovalRelay(ApiPerformance.GoalApprovalService.GoalApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncCorporateEventTotalsStatusRecord record, CancellationToken token)
    {
        await _client.CommentGoalApprovalAsync(new ApiPerformance.CommentGoalApprovalRequest { GoalId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskLifecycleService.</summary>
internal sealed class SyncCorporateEventTotalsStatusOnboardingTaskLifecycleTarget : ISyncCorporateEventTotalsStatusSink
{
    private readonly ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient _client;

    public SyncCorporateEventTotalsStatusOnboardingTaskLifecycleTarget(ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncCorporateEventTotalsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.RenewOnboardingTaskRequest { Reason = record.CorrelationId, OnboardingTaskId = record.Status }, token, _client.RenewOnboardingTaskAsync);
    }
}

/// <summary>Writes records to acme.itsm.v1.ChangeRequestLifecycleService.</summary>
internal sealed class SyncCorporateEventTotalsStatusChangeRequestLifecycleRelay(ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient client) : ISyncCorporateEventTotalsStatusSink
{
    public async Task ForwardAsync(SyncCorporateEventTotalsStatusRecord record, CancellationToken token)
    {
        await client.SuspendChangeRequestAsync(new ApiItsm.SuspendChangeRequestRequest { ChangeRequestId = record.Status, Details = new ApiItsm.SuspendChangeRequestRequest.Types.Details { ReasonCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventReportingService/CountCorporateEventTotals and hands what it keeps to the sink of its kind.</summary>
internal static class SyncCorporateEventTotalsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(channel);
        var response = await source.CountCorporateEventTotalsAsync(new ApiCorporateevents.CountCorporateEventTotalsRequest() { OrderBy = "live-sync-corporate-event-totals-status" }, cancellationToken: token);
        var record = new SyncCorporateEventTotalsStatusRecord
        {
            Status = response.Result.Status,
            CorrelationId = response.Result.CorrelationId,
            Message = Text.Normalize(response.Result.Message),
        };
        var sinks = new Dictionary<string, ISyncCorporateEventTotalsStatusSink>
        {
            ["goal-approval"] = new SyncCorporateEventTotalsStatusGoalApprovalRelay(new ApiPerformance.GoalApprovalService.GoalApprovalServiceClient(channel)),
            ["onboarding-task-lifecycle"] = new SyncCorporateEventTotalsStatusOnboardingTaskLifecycleTarget(new ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient(channel)),
            ["change-request-lifecycle"] = new SyncCorporateEventTotalsStatusChangeRequestLifecycleRelay(new ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "onboarding-task-lifecycle")
                continue;
            await sink.ForwardAsync(record, token);
        }
    }
}
