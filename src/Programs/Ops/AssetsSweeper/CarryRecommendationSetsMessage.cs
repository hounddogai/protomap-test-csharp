using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiSecrets = Acme.Secrets.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where CarryRecommendationSetsMessage hands each record.</summary>
internal interface ICarryRecommendationSetsMessageTarget
{
    Task ApplyAsync(CarryRecommendationSetsMessageRecord record, CancellationToken token);
}

/// <summary>What CarryRecommendationSetsMessage keeps of each SummarizeRecommendationSets response.</summary>
internal sealed class CarryRecommendationSetsMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Timezone { get; init; } = "";
}

/// <summary>Passes records to acme.corporateevents.v1.VenueWorkflowService.</summary>
internal sealed class CarryRecommendationSetsMessageVenueWorkflowTarget(ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient client) : ICarryRecommendationSetsMessageTarget
{
    public async Task ApplyAsync(CarryRecommendationSetsMessageRecord record, CancellationToken token)
    {
        await client.InspectVenueWorkflowAsync(new ApiCorporateevents.InspectVenueWorkflowRequest { VenueId = record.Timezone }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.TravelBookingQueryService.</summary>
internal sealed class CarryRecommendationSetsMessageTravelBookingQueryTarget(ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient client) : ICarryRecommendationSetsMessageTarget
{
    public async Task ApplyAsync(CarryRecommendationSetsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.SuggestTravelBookingsRequest { OrderBy = record.Message }, token, client.SuggestTravelBookingsAsync);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelBookingReportingService.</summary>
internal sealed class CarryRecommendationSetsMessageTravelBookingReportingRelay : ICarryRecommendationSetsMessageTarget
{
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _client;

    public CarryRecommendationSetsMessageTravelBookingReportingRelay(ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient client) => _client = client;

    public async Task ApplyAsync(CarryRecommendationSetsMessageRecord record, CancellationToken token)
    {
        await _client.GenerateTravelBookingReportAsync(new ApiTravel.GenerateTravelBookingReportRequest { Details = new ApiTravel.GenerateTravelBookingReportRequest.Types.Details { Cron = record.Timezone, Format = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.secrets.v1.SecretQueryService.</summary>
internal sealed class CarryRecommendationSetsMessageSecretQueryTarget(ApiSecrets.SecretQueryService.SecretQueryServiceClient client) : ICarryRecommendationSetsMessageTarget
{
    public async Task ApplyAsync(CarryRecommendationSetsMessageRecord record, CancellationToken token)
    {
        await client.SummarizeSecretsAsync(new ApiSecrets.SummarizeSecretsRequest { SecretId = record.Timezone, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.recommendations.v1.RecommendationSetQueryService/SummarizeRecommendationSets and hands what it keeps to the sink of its kind.</summary>
internal static class CarryRecommendationSetsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient(channel);
        var got = await source.SummarizeRecommendationSetsAsync(new ApiRecommendations.SummarizeRecommendationSetsRequest() { RecommendationSetId = "nightly-carry-recommendation-sets-message" }, cancellationToken: token);
        var record = new CarryRecommendationSetsMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = got.Result.CorrelationId,
            Timezone = got.Period.Timezone,
        };
        var sinks = new Dictionary<string, ICarryRecommendationSetsMessageTarget>
        {
            ["venue-workflow"] = new CarryRecommendationSetsMessageVenueWorkflowTarget(new ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient(channel)),
            ["travel-booking-query"] = new CarryRecommendationSetsMessageTravelBookingQueryTarget(new ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient(channel)),
            ["travel-booking-reporting"] = new CarryRecommendationSetsMessageTravelBookingReportingRelay(new ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient(channel)),
            ["secret-query"] = new CarryRecommendationSetsMessageSecretQueryTarget(new ApiSecrets.SecretQueryService.SecretQueryServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "secret-query")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
