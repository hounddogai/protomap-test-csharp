using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ReplayCarePlansStatus hands each record.</summary>
internal interface IReplayCarePlansStatusDestination
{
    Task WriteAsync(ReplayCarePlansStatusRecord record, CancellationToken token);
}

/// <summary>What ReplayCarePlansStatus keeps of each ListCarePlans response.</summary>
internal sealed class ReplayCarePlansStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.itsm.v1.ServiceAssetCalculationService.</summary>
internal sealed class ReplayCarePlansStatusServiceAssetCalculationSink(ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient client) : IReplayCarePlansStatusDestination
{
    public async Task WriteAsync(ReplayCarePlansStatusRecord record, CancellationToken token)
    {
        await client.SimulateServiceAssetAsync(new ApiItsm.SimulateServiceAssetRequest { ServiceAsset = new ApiItsm.ServiceAsset { Code = record.NextPageToken }, ServiceAssetId = record.CorrelationId, Details = new ApiItsm.SimulateServiceAssetRequest.Types.Details { CurrencyCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.ItineraryCalculationService.</summary>
internal sealed class ReplayCarePlansStatusItineraryCalculationRelay : IReplayCarePlansStatusDestination
{
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _client;

    public ReplayCarePlansStatusItineraryCalculationRelay(ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient client) => _client = client;

    public async Task WriteAsync(ReplayCarePlansStatusRecord record, CancellationToken token)
    {
        await _client.ExplainItineraryCalculationAsync(new ApiTravel.ExplainItineraryCalculationRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.venues.v1.VenueQueryService.</summary>
internal sealed class ReplayCarePlansStatusVenueQueryRelay : IReplayCarePlansStatusDestination
{
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _client;

    public ReplayCarePlansStatusVenueQueryRelay(ApiVenues.VenueQueryService.VenueQueryServiceClient client) => _client = client;

    public async Task WriteAsync(ReplayCarePlansStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiVenues.BrowseVenuesRequest { Page = new ApiCommon.PageRequest { PageToken = record.NextPageToken } }, token, _client.BrowseVenuesAsync);
    }
}

/// <summary>Reads acme.careplans.v1.CarePlanService/ListCarePlans and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayCarePlansStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCareplans.CarePlanService.CarePlanServiceClient(channel);
        var reply = await source.ListCarePlansAsync(new ApiCareplans.ListCarePlansRequest() { OrderBy = "live-replay-care-plans-status" }, cancellationToken: token);
        var record = new ReplayCarePlansStatusRecord
        {
            Status = Text.Normalize(reply.Result.Status),
            NextPageToken = reply.PageInfo.NextPageToken,
            Message = Text.Normalize(reply.Result.Message),
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
        };
        var sinks = new IReplayCarePlansStatusDestination[]
        {
            new ReplayCarePlansStatusServiceAssetCalculationSink(new ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient(channel)),
            new ReplayCarePlansStatusItineraryCalculationRelay(new ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient(channel)),
            new ReplayCarePlansStatusVenueQueryRelay(new ApiVenues.VenueQueryService.VenueQueryServiceClient(channel)),
        };
        await sinks[1].WriteAsync(record, token);
    }
}
