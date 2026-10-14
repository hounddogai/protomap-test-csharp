using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where PropagateConversationCalculationStatus hands each record.</summary>
internal interface IPropagateConversationCalculationStatusDestination
{
    Task DeliverAsync(PropagateConversationCalculationStatusRecord record, CancellationToken token);
}

/// <summary>What PropagateConversationCalculationStatus keeps of each ExplainConversationCalculation response.</summary>
internal sealed class PropagateConversationCalculationStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.corporateevents.v1.CorporateEventQueryService.</summary>
internal sealed class PropagateConversationCalculationStatusCorporateEventQuerySink(ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient client) : IPropagateConversationCalculationStatusDestination
{
    public async Task DeliverAsync(PropagateConversationCalculationStatusRecord record, CancellationToken token)
    {
        await client.SummarizeCorporateEventsAsync(new ApiCorporateevents.SummarizeCorporateEventsRequest { CorporateEventId = record.Message, Reason = record.Status, Details = new ApiCorporateevents.SummarizeCorporateEventsRequest.Types.Details { Cron = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.scheduling.v1.RosterWorkflowService.</summary>
internal sealed class PropagateConversationCalculationStatusRosterWorkflowWriter(ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient client) : IPropagateConversationCalculationStatusDestination
{
    public async Task DeliverAsync(PropagateConversationCalculationStatusRecord record, CancellationToken token)
    {
        await client.CancelRosterWorkflowAsync(new ApiScheduling.CancelRosterWorkflowRequest { Details = new ApiScheduling.CancelRosterWorkflowRequest.Types.Details { Step = record.Status }, RosterId = record.CorrelationId, Roster = new ApiScheduling.Roster { Number = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.messaging.v1.ConversationCalculationService/ExplainConversationCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateConversationCalculationStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient(channel);
        var response = await source.ExplainConversationCalculationAsync(new ApiMessaging.ExplainConversationCalculationRequest() { Reason = "primary-propagate-conversation-calculation-status" }, cancellationToken: token);
        var record = new PropagateConversationCalculationStatusRecord
        {
            Status = Text.Normalize(response.Outcome.Status),
            Message = response.Outcome.Message,
            CorrelationId = response.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IPropagateConversationCalculationStatusDestination>
        {
            ["corporate-event-query"] = new PropagateConversationCalculationStatusCorporateEventQuerySink(new ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient(channel)),
            ["roster-workflow"] = new PropagateConversationCalculationStatusRosterWorkflowWriter(new ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient(channel)),
        };
        await RoutePropagateConversationCalculationStatusAsync("corporate-event-query", record, sinks, token);
    }

    private static async Task RoutePropagateConversationCalculationStatusAsync(string kind, PropagateConversationCalculationStatusRecord record, IReadOnlyDictionary<string, IPropagateConversationCalculationStatusDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
