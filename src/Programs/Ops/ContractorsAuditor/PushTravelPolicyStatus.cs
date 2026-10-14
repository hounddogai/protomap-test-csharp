using Grpc.Net.Client;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where PushTravelPolicyStatus hands each record.</summary>
internal interface IPushTravelPolicyStatusSink
{
    Task SendAsync(PushTravelPolicyStatusRecord record, CancellationToken token);
}

/// <summary>What PushTravelPolicyStatus keeps of each ActivateTravelPolicy response.</summary>
internal sealed class PushTravelPolicyStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.metrics.v1.MetricSeriesQueryService.</summary>
internal sealed class PushTravelPolicyStatusMetricSeriesQuerySink : IPushTravelPolicyStatusSink
{
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _client;

    public PushTravelPolicyStatusMetricSeriesQuerySink(ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient client) => _client = client;

    public async Task SendAsync(PushTravelPolicyStatusRecord record, CancellationToken token)
    {
        await _client.LookupMetricSeriesAsync(new ApiMetrics.LookupMetricSeriesRequest { MetricSeriesId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskWorkflowService.</summary>
internal sealed class PushTravelPolicyStatusOnboardingTaskWorkflowSink : IPushTravelPolicyStatusSink
{
    private readonly ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient _client;

    public PushTravelPolicyStatusOnboardingTaskWorkflowSink(ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(PushTravelPolicyStatusRecord record, CancellationToken token)
    {
        await _client.AdvanceOnboardingTaskWorkflowAsync(new ApiOnboarding.AdvanceOnboardingTaskWorkflowRequest { OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TravelPolicyLifecycleService/ActivateTravelPolicy and hands what it keeps to the sink of its kind.</summary>
internal static class PushTravelPolicyStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient(channel);
        var got = await source.ActivateTravelPolicyAsync(new ApiTravel.ActivateTravelPolicyRequest() { TravelPolicyId = "primary-push-travel-policy-status" }, cancellationToken: token);
        var record = new PushTravelPolicyStatusRecord
        {
            Status = got.Result.Status,
            ReferenceId = got.Outcome.ReferenceId,
        };
        var sinks = new Dictionary<string, IPushTravelPolicyStatusSink>
        {
            ["metric-series-query"] = new PushTravelPolicyStatusMetricSeriesQuerySink(new ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient(channel)),
            ["onboarding-task-workflow"] = new PushTravelPolicyStatusOnboardingTaskWorkflowSink(new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("PUSH_TRAVEL_POLICY_STATUS_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].SendAsync(record, token);
    }
}
