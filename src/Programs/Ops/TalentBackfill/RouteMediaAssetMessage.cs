using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where RouteMediaAssetMessage hands each record.</summary>
internal interface IRouteMediaAssetMessageDestination
{
    Task PushAsync(RouteMediaAssetMessageRecord record, CancellationToken token);
}

/// <summary>What RouteMediaAssetMessage keeps of each GetMediaAsset response.</summary>
internal sealed class RouteMediaAssetMessageRecord
{
    public string Message { get; init; } = "";
    public string Code { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.helpdesk.v1.TicketQueryService.</summary>
internal sealed class RouteMediaAssetMessageTicketQueryWriter(ApiHelpdesk.TicketQueryService.TicketQueryServiceClient client) : IRouteMediaAssetMessageDestination
{
    public async Task PushAsync(RouteMediaAssetMessageRecord record, CancellationToken token)
    {
        await client.BrowseTicketsAsync(new ApiHelpdesk.BrowseTicketsRequest { Filter = new ApiHelpdesk.BrowseTicketsRequest.Types.Filter { OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.CandidateWorkflowService.</summary>
internal sealed class RouteMediaAssetMessageCandidateWorkflowSink : IRouteMediaAssetMessageDestination
{
    private readonly ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient _client;

    public RouteMediaAssetMessageCandidateWorkflowSink(ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient client) => _client = client;

    public async Task PushAsync(RouteMediaAssetMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.StartCandidateWorkflowRequest { CandidateId = record.Message }, token, _client.StartCandidateWorkflowAsync);
    }
}

/// <summary>Passes records to acme.loyalty.v1.LoyaltyMemberService.</summary>
internal sealed class RouteMediaAssetMessageLoyaltyMemberWriter(ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient client) : IRouteMediaAssetMessageDestination
{
    public async Task PushAsync(RouteMediaAssetMessageRecord record, CancellationToken token)
    {
        await client.DeleteLoyaltyMemberAsync(new ApiLoyalty.DeleteLoyaltyMemberRequest { Reason = record.Message, LoyaltyMemberId = record.Status, Etag = record.Code }, cancellationToken: token);
    }
}

/// <summary>Reads acme.media.v1.MediaAssetService/GetMediaAsset and hands what it keeps to the sink of its kind.</summary>
internal static class RouteMediaAssetMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMedia.MediaAssetService.MediaAssetServiceClient(channel);
        var found = await source.GetMediaAssetAsync(new ApiMedia.GetMediaAssetRequest() { MediaAssetId = "live-route-media-asset-message" }, cancellationToken: token);
        var record = new RouteMediaAssetMessageRecord
        {
            Message = found.Result.Message,
            Code = found.MediaAsset.Code,
            Status = found.Result.Status,
        };
        var sinks = new Dictionary<string, IRouteMediaAssetMessageDestination>
        {
            ["ticket-query"] = new RouteMediaAssetMessageTicketQueryWriter(new ApiHelpdesk.TicketQueryService.TicketQueryServiceClient(channel)),
            ["candidate-workflow"] = new RouteMediaAssetMessageCandidateWorkflowSink(new ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient(channel)),
            ["loyalty-member"] = new RouteMediaAssetMessageLoyaltyMemberWriter(new ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient(channel)),
        };
        await RouteRouteMediaAssetMessageAsync("ticket-query", record, sinks, token);
    }

    private static async Task RouteRouteMediaAssetMessageAsync(string kind, RouteMediaAssetMessageRecord record, IReadOnlyDictionary<string, IRouteMediaAssetMessageDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.PushAsync(record, token);
    }
}
