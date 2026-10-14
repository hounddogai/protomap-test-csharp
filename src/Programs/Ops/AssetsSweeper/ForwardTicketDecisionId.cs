using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where ForwardTicketDecisionId hands each record.</summary>
internal interface IForwardTicketDecisionIdTarget
{
    Task PushAsync(ForwardTicketDecisionIdRecord record, CancellationToken token);
}

/// <summary>What ForwardTicketDecisionId keeps of each ExplainTicketDecision response.</summary>
internal sealed class ForwardTicketDecisionIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.onboarding.v1.EquipmentCalculationService.</summary>
internal sealed class ForwardTicketDecisionIdEquipmentCalculationTarget(ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient client) : IForwardTicketDecisionIdTarget
{
    public async Task PushAsync(ForwardTicketDecisionIdRecord record, CancellationToken token)
    {
        await client.ExplainEquipmentCalculationAsync(new ApiOnboarding.ExplainEquipmentCalculationRequest { Reason = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.helpdesk.v1.KnowledgeArticleWorkflowService.</summary>
internal sealed class ForwardTicketDecisionIdKnowledgeArticleWorkflowTarget(ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient client) : IForwardTicketDecisionIdTarget
{
    public async Task PushAsync(ForwardTicketDecisionIdRecord record, CancellationToken token)
    {
        await client.CancelKnowledgeArticleWorkflowAsync(new ApiHelpdesk.CancelKnowledgeArticleWorkflowRequest { KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle { KnowledgeArticleId = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.timekeeping.v1.ClockEventQueryService.</summary>
internal sealed class ForwardTicketDecisionIdClockEventQueryWriter : IForwardTicketDecisionIdTarget
{
    private readonly ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient _client;

    public ForwardTicketDecisionIdClockEventQueryWriter(ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient client) => _client = client;

    public async Task PushAsync(ForwardTicketDecisionIdRecord record, CancellationToken token)
    {
        await _client.CountClockEventsAsync(new ApiTimekeeping.CountClockEventsRequest { Filter = new ApiTimekeeping.CountClockEventsRequest.Types.Filter { Region = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.helpdesk.v1.TicketValidationService/ExplainTicketDecision and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardTicketDecisionId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.TicketValidationService.TicketValidationServiceClient(channel);
        var response = await source.ExplainTicketDecisionAsync(new ApiHelpdesk.ExplainTicketDecisionRequest() { Reason = "live-forward-ticket-decision-id" }, cancellationToken: token);
        var record = new ForwardTicketDecisionIdRecord
        {
            ReferenceId = Text.Normalize(response.Outcome.ReferenceId),
            Message = response.Outcome.Message,
        };
        var sinks = new Dictionary<string, IForwardTicketDecisionIdTarget>
        {
            ["equipment-calculation"] = new ForwardTicketDecisionIdEquipmentCalculationTarget(new ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient(channel)),
            ["knowledge-article-workflow"] = new ForwardTicketDecisionIdKnowledgeArticleWorkflowTarget(new ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient(channel)),
            ["clock-event-query"] = new ForwardTicketDecisionIdClockEventQueryWriter(new ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "equipment-calculation")
                continue;
            await sink.PushAsync(record, token);
        }
    }
}
