using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiChat = Acme.Chat.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where ExportPendingTripsId hands each record.</summary>
internal interface IExportPendingTripsIdSink
{
    Task PushAsync(ExportPendingTripsIdRecord record, CancellationToken token);
}

/// <summary>What ExportPendingTripsId keeps of each ListPendingTrips response.</summary>
internal sealed class ExportPendingTripsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.travel.v1.TravelBookingQueryService.</summary>
internal sealed class ExportPendingTripsIdTravelBookingQueryTarget(ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient client) : IExportPendingTripsIdSink
{
    public async Task PushAsync(ExportPendingTripsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.LookupTravelBookingRequest { TravelBookingId = record.CorrelationId }, token, client.LookupTravelBookingAsync);
    }
}

/// <summary>Sends records to acme.itsm.v1.ServiceAssetWorkflowService.</summary>
internal sealed class ExportPendingTripsIdServiceAssetWorkflowRelay : IExportPendingTripsIdSink
{
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _client;

    public ExportPendingTripsIdServiceAssetWorkflowRelay(ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient client) => _client = client;

    public async Task PushAsync(ExportPendingTripsIdRecord record, CancellationToken token)
    {
        await _client.SignalServiceAssetWorkflowAsync(new ApiItsm.SignalServiceAssetWorkflowRequest { ServiceAssetId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.ratelimiting.v1.RateLimitCalculationService.</summary>
internal sealed class ExportPendingTripsIdRateLimitCalculationTarget : IExportPendingTripsIdSink
{
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _client;

    public ExportPendingTripsIdRateLimitCalculationTarget(ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient client) => _client = client;

    public async Task PushAsync(ExportPendingTripsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRatelimiting.EstimateRateLimitRequest { RateLimit = new ApiRatelimiting.RateLimit { Name = record.CorrelationId } }, token, _client.EstimateRateLimitAsync);
    }
}

/// <summary>Writes records to acme.chat.v1.ChatRoomQueryService.</summary>
internal sealed class ExportPendingTripsIdChatRoomQuerySink(ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient client) : IExportPendingTripsIdSink
{
    public async Task PushAsync(ExportPendingTripsIdRecord record, CancellationToken token)
    {
        await client.SuggestChatRoomsAsync(new ApiChat.SuggestChatRoomsRequest { Filter = new ApiChat.SuggestChatRoomsRequest.Types.Filter { OwnerId = record.CorrelationId, Region = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TripApprovalService/ListPendingTrips and hands what it keeps to the sink of its kind.</summary>
internal static class ExportPendingTripsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripApprovalService.TripApprovalServiceClient(channel);
        var got = await source.ListPendingTripsAsync(new ApiTravel.ListPendingTripsRequest() { OrderBy = "nightly-export-pending-trips-id" }, cancellationToken: token);
        var record = new ExportPendingTripsIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            Message = got.Result.Message,
        };
        await ActOnExportPendingTripsIdAsync("chat-room-query", record, channel, token);
    }

    private static async Task ActOnExportPendingTripsIdAsync(string kind, ExportPendingTripsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["travel-booking-query"] = () => new ExportPendingTripsIdTravelBookingQueryTarget(new ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient(channel)).PushAsync(record, token),
            ["asset-workflow"] = () => new ExportPendingTripsIdServiceAssetWorkflowRelay(new ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient(channel)).PushAsync(record, token),
            ["rate-limit-calculation"] = () => new ExportPendingTripsIdRateLimitCalculationTarget(new ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient(channel)).PushAsync(record, token),
            ["chat-room-query"] = () => new ExportPendingTripsIdChatRoomQuerySink(new ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
