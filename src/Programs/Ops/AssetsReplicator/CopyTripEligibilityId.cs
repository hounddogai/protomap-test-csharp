using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where CopyTripEligibilityId hands each record.</summary>
internal interface ICopyTripEligibilityIdTarget
{
    Task SendAsync(CopyTripEligibilityIdRecord record, CancellationToken token);
}

/// <summary>What CopyTripEligibilityId keeps of each CheckTripEligibility response.</summary>
internal sealed class CopyTripEligibilityIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Number { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskWorkflowService.</summary>
internal sealed class CopyTripEligibilityIdOnboardingTaskWorkflowTarget : ICopyTripEligibilityIdTarget
{
    private readonly ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient _client;

    public CopyTripEligibilityIdOnboardingTaskWorkflowTarget(ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(CopyTripEligibilityIdRecord record, CancellationToken token)
    {
        await _client.SignalOnboardingTaskWorkflowAsync(new ApiOnboarding.SignalOnboardingTaskWorkflowRequest { Details = new ApiOnboarding.SignalOnboardingTaskWorkflowRequest.Types.Details { InitiatedBy = record.Message, Step = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.metrics.v1.MetricSeriesService.</summary>
internal sealed class CopyTripEligibilityIdMetricSeriesTarget : ICopyTripEligibilityIdTarget
{
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _client;

    public CopyTripEligibilityIdMetricSeriesTarget(ApiMetrics.MetricSeriesService.MetricSeriesServiceClient client) => _client = client;

    public async Task SendAsync(CopyTripEligibilityIdRecord record, CancellationToken token)
    {
        await _client.GetMetricSeriesAsync(new ApiMetrics.GetMetricSeriesRequest { MetricSeriesId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.itsm.v1.ProblemSyncService.</summary>
internal sealed class CopyTripEligibilityIdProblemSyncSink : ICopyTripEligibilityIdTarget
{
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _client;

    public CopyTripEligibilityIdProblemSyncSink(ApiItsm.ProblemSyncService.ProblemSyncServiceClient client) => _client = client;

    public async Task SendAsync(CopyTripEligibilityIdRecord record, CancellationToken token)
    {
        await _client.ReconcileProblemsAsync(new ApiItsm.ReconcileProblemsRequest { Details = new ApiItsm.ReconcileProblemsRequest.Types.Details { Checksum = record.ReferenceId }, Problem = new ApiItsm.Problem { ProblemId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TripValidationService/CheckTripEligibility and hands what it keeps to the sink of its kind.</summary>
internal static class CopyTripEligibilityId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripValidationService.TripValidationServiceClient(channel);
        var response = await source.CheckTripEligibilityAsync(new ApiTravel.CheckTripEligibilityRequest() { Reason = "current-copy-trip-eligibility-id" }, cancellationToken: token);
        var record = new CopyTripEligibilityIdRecord
        {
            ReferenceId = response.Outcome.ReferenceId,
            Message = response.Result.Message,
            Number = Text.Normalize(response.Trip.Number),
            CorrelationId = response.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ICopyTripEligibilityIdTarget>
        {
            ["onboarding-task-workflow"] = new CopyTripEligibilityIdOnboardingTaskWorkflowTarget(new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel)),
            ["metric-series"] = new CopyTripEligibilityIdMetricSeriesTarget(new ApiMetrics.MetricSeriesService.MetricSeriesServiceClient(channel)),
            ["problem-sync"] = new CopyTripEligibilityIdProblemSyncSink(new ApiItsm.ProblemSyncService.ProblemSyncServiceClient(channel)),
        };
        await RouteCopyTripEligibilityIdAsync("onboarding-task-workflow", record, sinks, token);
    }

    private static async Task RouteCopyTripEligibilityIdAsync(string kind, CopyTripEligibilityIdRecord record, IReadOnlyDictionary<string, ICopyTripEligibilityIdTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
