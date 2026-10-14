using Grpc.Net.Client;
using ApiSeating = Acme.Seating.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>What RelayVenueCode keeps of each CloneVenue response.</summary>
internal sealed class RelayVenueCodeRecord
{
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.seating.v1.SeatMapQueryService.</summary>
internal sealed class RelayVenueCodeSeatMapQueryWriter(ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient client)
{
    public async Task DeliverAsync(RelayVenueCodeRecord record, CancellationToken token)
    {
        await client.FindSeatMapsAsync(new ApiSeating.FindSeatMapsRequest { Filter = new ApiSeating.FindSeatMapsRequest.Types.Filter { Region = record.Message }, OrderBy = record.Code }, cancellationToken: token);
    }
}

/// <summary>Reads acme.venues.v1.VenueService/CloneVenue and hands what it keeps to acme.seating.v1.SeatMapQueryService.</summary>
internal static class RelayVenueCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiVenues.VenueService.VenueServiceClient(channel);
        var got = await source.CloneVenueAsync(new ApiVenues.CloneVenueRequest() { RequestId = "default-relay-venue-code" }, cancellationToken: token);
        var record = new RelayVenueCodeRecord
        {
            Code = got.Venue.Code,
            Message = got.Result.Message,
        };
        await new RelayVenueCodeSeatMapQueryWriter(new ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient(channel)).DeliverAsync(record, token);
    }
}
