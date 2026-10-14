using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where ForwardCryptoKeyMessage hands each record.</summary>
internal interface IForwardCryptoKeyMessageTarget
{
    Task WriteAsync(ForwardCryptoKeyMessageRecord record, CancellationToken token);
}

/// <summary>What ForwardCryptoKeyMessage keeps of each CalculateCryptoKey response.</summary>
internal sealed class ForwardCryptoKeyMessageRecord
{
    public string Message { get; init; } = "";
    public string CryptoKeyId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.timekeeping.v1.ClockEventCalculationService.</summary>
internal sealed class ForwardCryptoKeyMessageClockEventCalculationRelay(ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient client) : IForwardCryptoKeyMessageTarget
{
    public async Task WriteAsync(ForwardCryptoKeyMessageRecord record, CancellationToken token)
    {
        await client.SimulateClockEventAsync(new ApiTimekeeping.SimulateClockEventRequest { Details = new ApiTimekeeping.SimulateClockEventRequest.Types.Details { CurrencyCode = record.Status }, ClockEvent = new ApiTimekeeping.ClockEvent { CorrelationId = record.CryptoKeyId }, ClockEventId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.ItineraryReportingService.</summary>
internal sealed class ForwardCryptoKeyMessageItineraryReportingSink : IForwardCryptoKeyMessageTarget
{
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _client;

    public ForwardCryptoKeyMessageItineraryReportingSink(ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient client) => _client = client;

    public async Task WriteAsync(ForwardCryptoKeyMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.GenerateItineraryReportRequest { Details = new ApiTravel.GenerateItineraryReportRequest.Types.Details { Format = record.Status } }, token, _client.GenerateItineraryReportAsync);
    }
}

/// <summary>Reads acme.keymanagement.v1.CryptoKeyCalculationService/CalculateCryptoKey and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardCryptoKeyMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient(channel);
        var found = await source.CalculateCryptoKeyAsync(new ApiKeymanagement.CalculateCryptoKeyRequest() { CryptoKeyId = "current-forward-crypto-key-message" }, cancellationToken: token);
        var record = new ForwardCryptoKeyMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            CryptoKeyId = found.CryptoKey.CryptoKeyId,
            Status = found.Result.Status,
        };
        var sinks = new Dictionary<string, IForwardCryptoKeyMessageTarget>
        {
            ["clock-event-calculation"] = new ForwardCryptoKeyMessageClockEventCalculationRelay(new ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient(channel)),
            ["itinerary-reporting"] = new ForwardCryptoKeyMessageItineraryReportingSink(new ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient(channel)),
        };
        await RouteForwardCryptoKeyMessageAsync("itinerary-reporting", record, sinks, token);
    }

    private static async Task RouteForwardCryptoKeyMessageAsync(string kind, ForwardCryptoKeyMessageRecord record, IReadOnlyDictionary<string, IForwardCryptoKeyMessageTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
