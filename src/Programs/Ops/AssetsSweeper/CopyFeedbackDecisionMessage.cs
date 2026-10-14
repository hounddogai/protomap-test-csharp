using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiConfiguration = Acme.Configuration.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where CopyFeedbackDecisionMessage hands each record.</summary>
internal interface ICopyFeedbackDecisionMessageTarget
{
    Task PushAsync(CopyFeedbackDecisionMessageRecord record, CancellationToken token);
}

/// <summary>What CopyFeedbackDecisionMessage keeps of each ExplainFeedbackDecision response.</summary>
internal sealed class CopyFeedbackDecisionMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.returns.v1.ReturnRequestQueryService.</summary>
internal sealed class CopyFeedbackDecisionMessageReturnRequestQueryWriter(ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient client) : ICopyFeedbackDecisionMessageTarget
{
    public async Task PushAsync(CopyFeedbackDecisionMessageRecord record, CancellationToken token)
    {
        await client.BrowseReturnRequestsAsync(new ApiReturns.BrowseReturnRequestsRequest { Filter = new ApiReturns.BrowseReturnRequestsRequest.Types.Filter { Region = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.onboarding.v1.OnboardingTaskValidationService.</summary>
internal sealed class CopyFeedbackDecisionMessageOnboardingTaskValidationSink(ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient client) : ICopyFeedbackDecisionMessageTarget
{
    public async Task PushAsync(CopyFeedbackDecisionMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.ExplainOnboardingTaskDecisionRequest { OnboardingTaskId = record.Message, OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.CorrelationId } }, token, client.ExplainOnboardingTaskDecisionAsync);
    }
}

/// <summary>Hands records to acme.configuration.v1.ConfigSetQueryService.</summary>
internal sealed class CopyFeedbackDecisionMessageConfigSetQueryWriter(ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient client) : ICopyFeedbackDecisionMessageTarget
{
    public async Task PushAsync(CopyFeedbackDecisionMessageRecord record, CancellationToken token)
    {
        await client.BrowseConfigSetsAsync(new ApiConfiguration.BrowseConfigSetsRequest { Filter = new ApiConfiguration.BrowseConfigSetsRequest.Types.Filter { OwnerId = record.CorrelationId }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.performance.v1.FeedbackValidationService/ExplainFeedbackDecision and hands what it keeps to the sink of its kind.</summary>
internal static class CopyFeedbackDecisionMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient(channel);
        var found = await source.ExplainFeedbackDecisionAsync(new ApiPerformance.ExplainFeedbackDecisionRequest() { Reason = "main-copy-feedback-decision-message" }, cancellationToken: token);
        var record = new CopyFeedbackDecisionMessageRecord
        {
            Message = found.Result.Message,
            CorrelationId = Text.Normalize(found.Feedback.CorrelationId),
        };
        var sinks = new Dictionary<string, ICopyFeedbackDecisionMessageTarget>
        {
            ["return-request-query"] = new CopyFeedbackDecisionMessageReturnRequestQueryWriter(new ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient(channel)),
            ["onboarding-task-validation"] = new CopyFeedbackDecisionMessageOnboardingTaskValidationSink(new ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient(channel)),
            ["config-set-query"] = new CopyFeedbackDecisionMessageConfigSetQueryWriter(new ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient(channel)),
        };
        await RouteCopyFeedbackDecisionMessageAsync("return-request-query", record, sinks, token);
    }

    private static async Task RouteCopyFeedbackDecisionMessageAsync(string kind, CopyFeedbackDecisionMessageRecord record, IReadOnlyDictionary<string, ICopyFeedbackDecisionMessageTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.PushAsync(record, token);
    }
}
