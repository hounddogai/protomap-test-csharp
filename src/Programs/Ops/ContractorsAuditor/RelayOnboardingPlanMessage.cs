using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where RelayOnboardingPlanMessage hands each record.</summary>
internal interface IRelayOnboardingPlanMessageWriter
{
    Task DeliverAsync(RelayOnboardingPlanMessageRecord record, CancellationToken token);
}

/// <summary>What RelayOnboardingPlanMessage keeps of each RenewOnboardingPlan response.</summary>
internal sealed class RelayOnboardingPlanMessageRecord
{
    public string Message { get; init; } = "";
    public string Number { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
}

/// <summary>Hands records to acme.helpdesk.v1.KnowledgeArticleApprovalService.</summary>
internal sealed class RelayOnboardingPlanMessageKnowledgeArticleApprovalSink : IRelayOnboardingPlanMessageWriter
{
    private readonly ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient _client;

    public RelayOnboardingPlanMessageKnowledgeArticleApprovalSink(ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(RelayOnboardingPlanMessageRecord record, CancellationToken token)
    {
        await _client.ListPendingKnowledgeArticlesAsync(new ApiHelpdesk.ListPendingKnowledgeArticlesRequest { Filter = new ApiHelpdesk.ListPendingKnowledgeArticlesRequest.Types.Filter { OwnerId = record.OutcomeMessage, Region = record.CorrelationId }, Page = new ApiCommon.PageRequest { PageToken = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class RelayOnboardingPlanMessageCheckoutSessionTarget(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) : IRelayOnboardingPlanMessageWriter
{
    public async Task DeliverAsync(RelayOnboardingPlanMessageRecord record, CancellationToken token)
    {
        await client.UpdateCheckoutSessionAsync(new ApiCheckout.UpdateCheckoutSessionRequest { CheckoutSession = new ApiCheckout.CheckoutSession { Notes = record.Message, Number = record.CorrelationId, CheckoutSessionId = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.RoomBookingCalculationService.</summary>
internal sealed class RelayOnboardingPlanMessageRoomBookingCalculationTarget(ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient client) : IRelayOnboardingPlanMessageWriter
{
    public async Task DeliverAsync(RelayOnboardingPlanMessageRecord record, CancellationToken token)
    {
        await client.GetRoomBookingRatesAsync(new ApiWorkplaces.GetRoomBookingRatesRequest { RoomBookingId = record.Number }, cancellationToken: token);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingPlanLifecycleService/RenewOnboardingPlan and hands what it keeps to the sink of its kind.</summary>
internal static class RelayOnboardingPlanMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingPlanLifecycleService.OnboardingPlanLifecycleServiceClient(channel);
        var response = await source.RenewOnboardingPlanAsync(new ApiOnboarding.RenewOnboardingPlanRequest() { Reason = "nightly-relay-onboarding-plan-message" }, cancellationToken: token);
        var record = new RelayOnboardingPlanMessageRecord
        {
            Message = Text.Normalize(response.Result.Message),
            Number = response.OnboardingPlan.Number,
            CorrelationId = response.Result.CorrelationId,
            OutcomeMessage = response.Outcome.Message,
        };
        await ActOnRelayOnboardingPlanMessageAsync("room-booking-calculation", record, channel, token);
    }

    private static async Task ActOnRelayOnboardingPlanMessageAsync(string kind, RelayOnboardingPlanMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["knowledge-article-approval"] = () => new RelayOnboardingPlanMessageKnowledgeArticleApprovalSink(new ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient(channel)).DeliverAsync(record, token),
            ["checkout-session"] = () => new RelayOnboardingPlanMessageCheckoutSessionTarget(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel)).DeliverAsync(record, token),
            ["room-booking-calculation"] = () => new RelayOnboardingPlanMessageRoomBookingCalculationTarget(new ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
