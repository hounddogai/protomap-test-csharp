using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where MirrorClockEventMessage hands each record.</summary>
internal interface IMirrorClockEventMessageSink
{
    Task SendAsync(MirrorClockEventMessageRecord record, CancellationToken token);
}

/// <summary>What MirrorClockEventMessage keeps of each PreviewClockEvent response.</summary>
internal sealed class MirrorClockEventMessageRecord
{
    public string Message { get; init; } = "";
    public string Kind { get; init; } = "";
}

/// <summary>Passes records to acme.ratelimiting.v1.RateLimitQueryService.</summary>
internal sealed class MirrorClockEventMessageRateLimitQueryRelay : IMirrorClockEventMessageSink
{
    private readonly ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient _client;

    public MirrorClockEventMessageRateLimitQueryRelay(ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient client) => _client = client;

    public async Task SendAsync(MirrorClockEventMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRatelimiting.BrowseRateLimitsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Message }, Filter = new ApiRatelimiting.BrowseRateLimitsRequest.Types.Filter { OwnerId = record.Kind } }, token, _client.BrowseRateLimitsAsync);
    }
}

/// <summary>Hands records to acme.workplaces.v1.RoomBookingValidationService.</summary>
internal sealed class MirrorClockEventMessageRoomBookingValidationWriter : IMirrorClockEventMessageSink
{
    private readonly ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient _client;

    public MirrorClockEventMessageRoomBookingValidationWriter(ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient client) => _client = client;

    public async Task SendAsync(MirrorClockEventMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.NormalizeRoomBookingRequest { RoomBooking = new ApiWorkplaces.RoomBooking { RoomBookingId = record.Kind }, RoomBookingId = record.Message }, token, _client.NormalizeRoomBookingAsync);
    }
}

/// <summary>Reads acme.timekeeping.v1.ClockEventValidationService/PreviewClockEvent and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorClockEventMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(channel);
        var response = await source.PreviewClockEventAsync(new ApiTimekeeping.PreviewClockEventRequest() { ClockEventId = "live-mirror-clock-event-message" }, cancellationToken: token);
        var record = new MirrorClockEventMessageRecord
        {
            Message = response.Result.Message,
            Kind = response.ClockEvent.Kind,
        };
        var sinks = new IMirrorClockEventMessageSink[]
        {
            new MirrorClockEventMessageRateLimitQueryRelay(new ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient(channel)),
            new MirrorClockEventMessageRoomBookingValidationWriter(new ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient(channel)),
        };
        await sinks[0].SendAsync(record, token);
    }
}
