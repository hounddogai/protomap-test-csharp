using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiReviews = Acme.Reviews.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where PropagateOnboardingTasksCode hands each record.</summary>
internal interface IPropagateOnboardingTasksCodeDestination
{
    Task WriteAsync(PropagateOnboardingTasksCodeRecord record, CancellationToken token);
}

/// <summary>What PropagateOnboardingTasksCode keeps of each RecalculateOnboardingTasks response.</summary>
internal sealed class PropagateOnboardingTasksCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Hands records to acme.reviews.v1.ProductReviewQueryService.</summary>
internal sealed class PropagateOnboardingTasksCodeProductReviewQueryRelay : IPropagateOnboardingTasksCodeDestination
{
    private readonly ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient _client;

    public PropagateOnboardingTasksCodeProductReviewQueryRelay(ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateOnboardingTasksCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiReviews.CountProductReviewsRequest { OrderBy = record.Subject, Filter = new ApiReviews.CountProductReviewsRequest.Types.Filter { Region = record.CurrencyCode } }, token, _client.CountProductReviewsAsync);
    }
}

/// <summary>Writes records to acme.corporateevents.v1.CorporateEventApprovalService.</summary>
internal sealed class PropagateOnboardingTasksCodeCorporateEventApprovalSink : IPropagateOnboardingTasksCodeDestination
{
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _client;

    public PropagateOnboardingTasksCodeCorporateEventApprovalSink(ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateOnboardingTasksCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.SubmitCorporateEventRequest { Details = new ApiCorporateevents.SubmitCorporateEventRequest.Types.Details { Comment = record.CurrencyCode } }, token, _client.SubmitCorporateEventAsync);
    }
}

/// <summary>Passes records to acme.messaging.v1.ConversationCalculationService.</summary>
internal sealed class PropagateOnboardingTasksCodeConversationCalculationWriter : IPropagateOnboardingTasksCodeDestination
{
    private readonly ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient _client;

    public PropagateOnboardingTasksCodeConversationCalculationWriter(ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateOnboardingTasksCodeRecord record, CancellationToken token)
    {
        await _client.ExplainConversationCalculationAsync(new ApiMessaging.ExplainConversationCalculationRequest { ConversationId = record.Subject, Details = new ApiMessaging.ExplainConversationCalculationRequest.Types.Details { Locale = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateOnboardingTasksCodeLogOnly : IPropagateOnboardingTasksCodeDestination
{
    public Task WriteAsync(PropagateOnboardingTasksCodeRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CurrencyCode}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskCalculationService/RecalculateOnboardingTasks and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateOnboardingTasksCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(channel);
        var got = await source.RecalculateOnboardingTasksAsync(new ApiOnboarding.RecalculateOnboardingTasksRequest() { Reason = "primary-propagate-onboarding-tasks-code" }, cancellationToken: token);
        var record = new PropagateOnboardingTasksCodeRecord
        {
            CurrencyCode = Text.Normalize(got.Breakdown.CurrencyCode),
            Subject = Text.Normalize(got.OnboardingTask.Subject),
        };
        await DeliverPropagateOnboardingTasksCodeAsync("corporate-event-approval", record, channel, token);
    }

    private static async Task DeliverPropagateOnboardingTasksCodeAsync(string kind, PropagateOnboardingTasksCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagateOnboardingTasksCodeDestination sink = kind switch
        {
            "product-review-query" => new PropagateOnboardingTasksCodeProductReviewQueryRelay(new ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient(channel)),
            "corporate-event-approval" => new PropagateOnboardingTasksCodeCorporateEventApprovalSink(new ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient(channel)),
            "conversation-calculation" => new PropagateOnboardingTasksCodeConversationCalculationWriter(new ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient(channel)),
            _ => new PropagateOnboardingTasksCodeLogOnly(),
        };
        await sink.WriteAsync(record, token);
    }
}
