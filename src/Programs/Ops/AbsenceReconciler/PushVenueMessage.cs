using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiContent = Acme.Content.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where PushVenueMessage hands each record.</summary>
internal interface IPushVenueMessageDestination
{
    Task SendAsync(PushVenueMessageRecord record, CancellationToken token);
}

/// <summary>What PushVenueMessage keeps of each EstimateVenue response.</summary>
internal sealed class PushVenueMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.concessions.v1.ConcessionStandQueryService.</summary>
internal sealed class PushVenueMessageConcessionStandQuerySink : IPushVenueMessageDestination
{
    private readonly ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient _client;

    public PushVenueMessageConcessionStandQuerySink(ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient client) => _client = client;

    public async Task SendAsync(PushVenueMessageRecord record, CancellationToken token)
    {
        await _client.SuggestConcessionStandsAsync(new ApiConcessions.SuggestConcessionStandsRequest { Filter = new ApiConcessions.SuggestConcessionStandsRequest.Types.Filter { Region = record.CorrelationId }, Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.content.v1.ArticleQueryService.</summary>
internal sealed class PushVenueMessageArticleQueryTarget(ApiContent.ArticleQueryService.ArticleQueryServiceClient client) : IPushVenueMessageDestination
{
    public async Task SendAsync(PushVenueMessageRecord record, CancellationToken token)
    {
        await client.SuggestArticlesAsync(new ApiContent.SuggestArticlesRequest { Filter = new ApiContent.SuggestArticlesRequest.Types.Filter { OwnerId = record.CorrelationId, Region = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.VenueCalculationService/EstimateVenue and hands what it keeps to the sink of its kind.</summary>
internal static class PushVenueMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient(channel);
        var reply = await source.EstimateVenueAsync(new ApiCorporateevents.EstimateVenueRequest() { VenueId = "primary-push-venue-message" }, cancellationToken: token);
        var record = new PushVenueMessageRecord
        {
            Message = Text.Normalize(reply.Result.Message),
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
        };
        var sinks = new Dictionary<string, IPushVenueMessageDestination>
        {
            ["concession-stand-query"] = new PushVenueMessageConcessionStandQuerySink(new ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient(channel)),
            ["article-query"] = new PushVenueMessageArticleQueryTarget(new ApiContent.ArticleQueryService.ArticleQueryServiceClient(channel)),
        };
        await RoutePushVenueMessageAsync("concession-stand-query", record, sinks, token);
    }

    private static async Task RoutePushVenueMessageAsync(string kind, PushVenueMessageRecord record, IReadOnlyDictionary<string, IPushVenueMessageDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
