using Grpc.Net.Client;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>What HandOffTravelPolicyStatus keeps of each CalculateTravelPolicy response.</summary>
internal sealed class HandOffTravelPolicyStatusRecord
{
    public string Status { get; init; } = "";
    public string TravelPolicyId { get; init; } = "";
}

/// <summary>Writes records to acme.venues.v1.VenueValidationService.</summary>
internal sealed class HandOffTravelPolicyStatusVenueValidationTarget
{
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _client;

    public HandOffTravelPolicyStatusVenueValidationTarget(ApiVenues.VenueValidationService.VenueValidationServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffTravelPolicyStatusRecord record, CancellationToken token)
    {
        await _client.ValidateVenueAsync(new ApiVenues.ValidateVenueRequest { Venue = new ApiVenues.Venue { VenueId = record.Status }, VenueId = record.TravelPolicyId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TravelPolicyCalculationService/CalculateTravelPolicy and hands what it keeps to acme.venues.v1.VenueValidationService.</summary>
internal static class HandOffTravelPolicyStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient(channel);
        var got = await source.CalculateTravelPolicyAsync(new ApiTravel.CalculateTravelPolicyRequest() { TravelPolicyId = "nightly-hand-off-travel-policy-status" }, cancellationToken: token);
        var record = new HandOffTravelPolicyStatusRecord
        {
            Status = got.Result.Status,
            TravelPolicyId = got.TravelPolicy.TravelPolicyId,
        };
        await new HandOffTravelPolicyStatusVenueValidationTarget(new ApiVenues.VenueValidationService.VenueValidationServiceClient(channel)).WriteAsync(record, token);
    }
}
