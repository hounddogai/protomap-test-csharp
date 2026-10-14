using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCampaigns = Acme.Campaigns.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where PushTicketRulesMessage hands each record.</summary>
internal interface IPushTicketRulesMessageSink
{
    Task ForwardAsync(PushTicketRulesMessageRecord record, CancellationToken token);
}

/// <summary>What PushTicketRulesMessage keeps of each GetTicketRules response.</summary>
internal sealed class PushTicketRulesMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.campaigns.v1.MarketingCampaignService.</summary>
internal sealed class PushTicketRulesMessageMarketingCampaignSink : IPushTicketRulesMessageSink
{
    private readonly ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient _client;

    public PushTicketRulesMessageMarketingCampaignSink(ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient client) => _client = client;

    public async Task ForwardAsync(PushTicketRulesMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCampaigns.GetMarketingCampaignRequest { MarketingCampaignId = record.CorrelationId }, token, _client.GetMarketingCampaignAsync);
    }
}

/// <summary>Passes records to acme.itsm.v1.IncidentApprovalService.</summary>
internal sealed class PushTicketRulesMessageIncidentApprovalTarget(ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient client) : IPushTicketRulesMessageSink
{
    public async Task ForwardAsync(PushTicketRulesMessageRecord record, CancellationToken token)
    {
        await client.CommentIncidentApprovalAsync(new ApiItsm.CommentIncidentApprovalRequest { Incident = new ApiItsm.Incident { Subject = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.ItineraryValidationService.</summary>
internal sealed class PushTicketRulesMessageItineraryValidationSink(ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient client) : IPushTicketRulesMessageSink
{
    public async Task ForwardAsync(PushTicketRulesMessageRecord record, CancellationToken token)
    {
        await client.ExplainItineraryDecisionAsync(new ApiTravel.ExplainItineraryDecisionRequest { Details = new ApiTravel.ExplainItineraryDecisionRequest.Types.Details { Locale = record.Message }, Itinerary = new ApiTravel.Itinerary { Notes = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.helpdesk.v1.TicketValidationService/GetTicketRules and hands what it keeps to the sink of its kind.</summary>
internal static class PushTicketRulesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.TicketValidationService.TicketValidationServiceClient(channel);
        var reply = await source.GetTicketRulesAsync(new ApiHelpdesk.GetTicketRulesRequest() { TicketId = "default-push-ticket-rules-message" }, cancellationToken: token);
        var record = new PushTicketRulesMessageRecord
        {
            Message = reply.Result.Message,
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
        };
        var sinks = new Dictionary<string, IPushTicketRulesMessageSink>
        {
            ["marketing-campaign"] = new PushTicketRulesMessageMarketingCampaignSink(new ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient(channel)),
            ["incident-approval"] = new PushTicketRulesMessageIncidentApprovalTarget(new ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient(channel)),
            ["itinerary-validation"] = new PushTicketRulesMessageItineraryValidationSink(new ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient(channel)),
        };
        await RoutePushTicketRulesMessageAsync("itinerary-validation", record, sinks, token);
    }

    private static async Task RoutePushTicketRulesMessageAsync(string kind, PushTicketRulesMessageRecord record, IReadOnlyDictionary<string, IPushTicketRulesMessageSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
