using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCheckout = Acme.Checkout.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecommendations = Acme.Recommendations.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where ExportOnboardingTaskWorkflowSubject hands each record.</summary>
internal interface IExportOnboardingTaskWorkflowSubjectDestination
{
    Task PushAsync(ExportOnboardingTaskWorkflowSubjectRecord record, CancellationToken token);
}

/// <summary>What ExportOnboardingTaskWorkflowSubject keeps of each RetryOnboardingTaskWorkflow response.</summary>
internal sealed class ExportOnboardingTaskWorkflowSubjectRecord
{
    public string Subject { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.recommendations.v1.RecommendationSetService.</summary>
internal sealed class ExportOnboardingTaskWorkflowSubjectRecommendationSetTarget : IExportOnboardingTaskWorkflowSubjectDestination
{
    private readonly ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient _client;

    public ExportOnboardingTaskWorkflowSubjectRecommendationSetTarget(ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient client) => _client = client;

    public async Task PushAsync(ExportOnboardingTaskWorkflowSubjectRecord record, CancellationToken token)
    {
        await _client.SearchRecommendationSetsAsync(new ApiRecommendations.SearchRecommendationSetsRequest { Filter = new ApiRecommendations.SearchRecommendationSetsRequest.Types.Filter { Query = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class ExportOnboardingTaskWorkflowSubjectCheckoutSessionSink(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) : IExportOnboardingTaskWorkflowSubjectDestination
{
    public async Task PushAsync(ExportOnboardingTaskWorkflowSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCheckout.UpdateCheckoutSessionRequest { CheckoutSession = new ApiCheckout.CheckoutSession { Number = record.Message } }, token, client.UpdateCheckoutSessionAsync);
    }
}

/// <summary>Passes records to acme.itsm.v1.ProblemValidationService.</summary>
internal sealed class ExportOnboardingTaskWorkflowSubjectProblemValidationTarget(ApiItsm.ProblemValidationService.ProblemValidationServiceClient client) : IExportOnboardingTaskWorkflowSubjectDestination
{
    public async Task PushAsync(ExportOnboardingTaskWorkflowSubjectRecord record, CancellationToken token)
    {
        await client.ScoreProblemAsync(new ApiItsm.ScoreProblemRequest { Details = new ApiItsm.ScoreProblemRequest.Types.Details { CurrencyCode = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskWorkflowService/RetryOnboardingTaskWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class ExportOnboardingTaskWorkflowSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel);
        var found = await source.RetryOnboardingTaskWorkflowAsync(new ApiOnboarding.RetryOnboardingTaskWorkflowRequest() { OnboardingTaskId = "live-export-onboarding-task-workflow-subject" }, cancellationToken: token);
        var record = new ExportOnboardingTaskWorkflowSubjectRecord
        {
            Subject = Text.Normalize(found.OnboardingTask.Subject),
            Message = Text.Normalize(found.Outcome.Message),
        };
        var sinks = new IExportOnboardingTaskWorkflowSubjectDestination[]
        {
            new ExportOnboardingTaskWorkflowSubjectRecommendationSetTarget(new ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient(channel)),
            new ExportOnboardingTaskWorkflowSubjectCheckoutSessionSink(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel)),
            new ExportOnboardingTaskWorkflowSubjectProblemValidationTarget(new ApiItsm.ProblemValidationService.ProblemValidationServiceClient(channel)),
        };
        await sinks[0].PushAsync(record, token);
    }
}
