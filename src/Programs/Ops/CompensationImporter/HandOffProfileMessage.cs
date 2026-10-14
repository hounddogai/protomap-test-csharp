using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where HandOffProfileMessage hands each record.</summary>
internal interface IHandOffProfileMessageTarget
{
    Task DeliverAsync(HandOffProfileMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffProfileMessage keeps of each SimulateProfile response.</summary>
internal sealed class HandOffProfileMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.travel.v1.ItineraryReportingService.</summary>
internal sealed class HandOffProfileMessageItineraryReportingRelay(ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient client) : IHandOffProfileMessageTarget
{
    public async Task DeliverAsync(HandOffProfileMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.DeleteItineraryReportRequest { Etag = record.CorrelationId }, token, client.DeleteItineraryReportAsync);
    }
}

/// <summary>Writes records to acme.giftcards.v1.GiftCardService.</summary>
internal sealed class HandOffProfileMessageGiftCardSink : IHandOffProfileMessageTarget
{
    private readonly ApiGiftcards.GiftCardService.GiftCardServiceClient _client;

    public HandOffProfileMessageGiftCardSink(ApiGiftcards.GiftCardService.GiftCardServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffProfileMessageRecord record, CancellationToken token)
    {
        await _client.UpdateGiftCardAsync(new ApiGiftcards.UpdateGiftCardRequest { GiftCard = new ApiGiftcards.GiftCard { CurrencyCode = record.Status, GiftCardId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.personalization.v1.ProfileCalculationService/SimulateProfile and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffProfileMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient(channel);
        var found = await source.SimulateProfileAsync(new ApiPersonalization.SimulateProfileRequest() { ProfileId = "default-hand-off-profile-message" }, cancellationToken: token);
        var record = new HandOffProfileMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            Status = found.Result.Status,
        };
        var sinks = new Dictionary<string, IHandOffProfileMessageTarget>
        {
            ["itinerary-reporting"] = new HandOffProfileMessageItineraryReportingRelay(new ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient(channel)),
            ["gift-card"] = new HandOffProfileMessageGiftCardSink(new ApiGiftcards.GiftCardService.GiftCardServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "itinerary-reporting")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
