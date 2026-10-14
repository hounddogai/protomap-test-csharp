using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiFuel = Acme.Fuel.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where PushFuelCardMessage hands each record.</summary>
internal interface IPushFuelCardMessageSink
{
    Task SendAsync(PushFuelCardMessageRecord record, CancellationToken token);
}

/// <summary>What PushFuelCardMessage keeps of each LookupFuelCard response.</summary>
internal sealed class PushFuelCardMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Writes records to acme.travel.v1.TravelBookingValidationService.</summary>
internal sealed class PushFuelCardMessageTravelBookingValidationSink : IPushFuelCardMessageSink
{
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _client;

    public PushFuelCardMessageTravelBookingValidationSink(ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient client) => _client = client;

    public async Task SendAsync(PushFuelCardMessageRecord record, CancellationToken token)
    {
        await _client.NormalizeTravelBookingAsync(new ApiTravel.NormalizeTravelBookingRequest { Reason = record.CorrelationId, TravelBooking = new ApiTravel.TravelBooking { Number = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.performance.v1.FeedbackValidationService.</summary>
internal sealed class PushFuelCardMessageFeedbackValidationWriter : IPushFuelCardMessageSink
{
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _client;

    public PushFuelCardMessageFeedbackValidationWriter(ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient client) => _client = client;

    public async Task SendAsync(PushFuelCardMessageRecord record, CancellationToken token)
    {
        await _client.ScoreFeedbackAsync(new ApiPerformance.ScoreFeedbackRequest { Feedback = new ApiPerformance.Feedback { FeedbackId = record.Message, CorrelationId = record.CorrelationId }, Reason = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.ReviewApprovalService.</summary>
internal sealed class PushFuelCardMessageReviewApprovalTarget : IPushFuelCardMessageSink
{
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _client;

    public PushFuelCardMessageReviewApprovalTarget(ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient client) => _client = client;

    public async Task SendAsync(PushFuelCardMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.ApproveReviewRequest { Details = new ApiPerformance.ApproveReviewRequest.Types.Details { ApproverEmail = record.CorrelationId }, Review = new ApiPerformance.Review { Notes = record.CurrencyCode, ReviewId = record.Message } }, token, _client.ApproveReviewAsync);
    }
}

/// <summary>Reads acme.fuel.v1.FuelCardQueryService/LookupFuelCard and hands what it keeps to the sink of its kind.</summary>
internal static class PushFuelCardMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient(channel);
        var reply = await source.LookupFuelCardAsync(new ApiFuel.LookupFuelCardRequest() { FuelCardId = "main-push-fuel-card-message" }, cancellationToken: token);
        var record = new PushFuelCardMessageRecord
        {
            Message = reply.Result.Message,
            CorrelationId = reply.Result.CorrelationId,
            CurrencyCode = reply.FuelCard.CurrencyCode,
        };
        var sinks = new Dictionary<string, IPushFuelCardMessageSink>
        {
            ["travel-booking-validation"] = new PushFuelCardMessageTravelBookingValidationSink(new ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient(channel)),
            ["feedback-validation"] = new PushFuelCardMessageFeedbackValidationWriter(new ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient(channel)),
            ["review-approval"] = new PushFuelCardMessageReviewApprovalTarget(new ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient(channel)),
        };
        await RoutePushFuelCardMessageAsync("feedback-validation", record, sinks, token);
    }

    private static async Task RoutePushFuelCardMessageAsync(string kind, PushFuelCardMessageRecord record, IReadOnlyDictionary<string, IPushFuelCardMessageSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
