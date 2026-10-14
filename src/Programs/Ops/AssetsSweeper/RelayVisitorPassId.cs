using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where RelayVisitorPassId hands each record.</summary>
internal interface IRelayVisitorPassIdDestination
{
    Task PushAsync(RelayVisitorPassIdRecord record, CancellationToken token);
}

/// <summary>What RelayVisitorPassId keeps of each RecallVisitorPass response.</summary>
internal sealed class RelayVisitorPassIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string VisitorPassId { get; init; } = "";
    public string ResultMessage { get; init; } = "";
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassSyncService.</summary>
internal sealed class RelayVisitorPassIdVisitorPassSyncTarget(ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient client) : IRelayVisitorPassIdDestination
{
    public async Task PushAsync(RelayVisitorPassIdRecord record, CancellationToken token)
    {
        await client.PushVisitorPassAsync(new ApiWorkplaces.PushVisitorPassRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.performance.v1.FeedbackCalculationService.</summary>
internal sealed class RelayVisitorPassIdFeedbackCalculationWriter(ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient client) : IRelayVisitorPassIdDestination
{
    public async Task PushAsync(RelayVisitorPassIdRecord record, CancellationToken token)
    {
        await client.QuoteFeedbackAsync(new ApiPerformance.QuoteFeedbackRequest { Feedback = new ApiPerformance.Feedback { Source = record.VisitorPassId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.loyalty.v1.LoyaltyMemberService.</summary>
internal sealed class RelayVisitorPassIdLoyaltyMemberTarget : IRelayVisitorPassIdDestination
{
    private readonly ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient _client;

    public RelayVisitorPassIdLoyaltyMemberTarget(ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient client) => _client = client;

    public async Task PushAsync(RelayVisitorPassIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLoyalty.GetLoyaltyMemberRequest { LoyaltyMemberId = record.VisitorPassId }, token, _client.GetLoyaltyMemberAsync);
    }
}

/// <summary>Reads acme.workplaces.v1.VisitorPassApprovalService/RecallVisitorPass and hands what it keeps to the sink of its kind.</summary>
internal static class RelayVisitorPassId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient(channel);
        var reply = await source.RecallVisitorPassAsync(new ApiWorkplaces.RecallVisitorPassRequest() { Reason = "nightly-relay-visitor-pass-id" }, cancellationToken: token);
        var record = new RelayVisitorPassIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Message = reply.Outcome.Message,
            VisitorPassId = reply.VisitorPass.VisitorPassId,
            ResultMessage = reply.Result.Message,
        };
        var sinks = new Dictionary<string, IRelayVisitorPassIdDestination>
        {
            ["visitor-pass-sync"] = new RelayVisitorPassIdVisitorPassSyncTarget(new ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient(channel)),
            ["feedback-calculation"] = new RelayVisitorPassIdFeedbackCalculationWriter(new ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient(channel)),
            ["loyalty-member"] = new RelayVisitorPassIdLoyaltyMemberTarget(new ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient(channel)),
        };
        await RouteRelayVisitorPassIdAsync("feedback-calculation", record, sinks, token);
    }

    private static async Task RouteRelayVisitorPassIdAsync(string kind, RelayVisitorPassIdRecord record, IReadOnlyDictionary<string, IRelayVisitorPassIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.PushAsync(record, token);
    }
}
