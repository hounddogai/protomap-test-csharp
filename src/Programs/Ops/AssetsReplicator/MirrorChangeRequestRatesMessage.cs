using Grpc.Net.Client;
using ApiConcessions = Acme.Concessions.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecommendations = Acme.Recommendations.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where MirrorChangeRequestRatesMessage hands each record.</summary>
internal interface IMirrorChangeRequestRatesMessageWriter
{
    Task ApplyAsync(MirrorChangeRequestRatesMessageRecord record, CancellationToken token);
}

/// <summary>What MirrorChangeRequestRatesMessage keeps of each GetChangeRequestRates response.</summary>
internal sealed class MirrorChangeRequestRatesMessageRecord
{
    public string Message { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Passes records to acme.concessions.v1.ConcessionStandService.</summary>
internal sealed class MirrorChangeRequestRatesMessageConcessionStandSink : IMirrorChangeRequestRatesMessageWriter
{
    private readonly ApiConcessions.ConcessionStandService.ConcessionStandServiceClient _client;

    public MirrorChangeRequestRatesMessageConcessionStandSink(ApiConcessions.ConcessionStandService.ConcessionStandServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorChangeRequestRatesMessageRecord record, CancellationToken token)
    {
        await _client.CreateConcessionStandAsync(new ApiConcessions.CreateConcessionStandRequest { ConcessionStand = new ApiConcessions.ConcessionStand { Code = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recommendations.v1.RecommendationSetService.</summary>
internal sealed class MirrorChangeRequestRatesMessageRecommendationSetSink(ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient client) : IMirrorChangeRequestRatesMessageWriter
{
    public async Task ApplyAsync(MirrorChangeRequestRatesMessageRecord record, CancellationToken token)
    {
        await client.GetRecommendationSetAsync(new ApiRecommendations.GetRecommendationSetRequest { RecommendationSetId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ChangeRequestCalculationService/GetChangeRequestRates and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorChangeRequestRatesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient(channel);
        var reply = await source.GetChangeRequestRatesAsync(new ApiItsm.GetChangeRequestRatesRequest() { ChangeRequestId = "current-mirror-change-request-rates-message" }, cancellationToken: token);
        var record = new MirrorChangeRequestRatesMessageRecord
        {
            Message = reply.Result.Message,
            Subject = reply.ChangeRequest.Subject,
        };
        var sinks = new Dictionary<string, IMirrorChangeRequestRatesMessageWriter>
        {
            ["concession-stand"] = new MirrorChangeRequestRatesMessageConcessionStandSink(new ApiConcessions.ConcessionStandService.ConcessionStandServiceClient(channel)),
            ["recommendation-set"] = new MirrorChangeRequestRatesMessageRecommendationSetSink(new ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient(channel)),
        };
        await RouteMirrorChangeRequestRatesMessageAsync("recommendation-set", record, sinks, token);
    }

    private static async Task RouteMirrorChangeRequestRatesMessageAsync(string kind, MirrorChangeRequestRatesMessageRecord record, IReadOnlyDictionary<string, IMirrorChangeRequestRatesMessageWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
