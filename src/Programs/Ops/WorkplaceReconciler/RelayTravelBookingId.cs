using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTicketing = Acme.Ticketing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where RelayTravelBookingId hands each record.</summary>
internal interface IRelayTravelBookingIdSink
{
    Task ForwardAsync(RelayTravelBookingIdRecord record, CancellationToken token);
}

/// <summary>What RelayTravelBookingId keeps of each SubmitTravelBooking response.</summary>
internal sealed class RelayTravelBookingIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.ratelimiting.v1.RateLimitService.</summary>
internal sealed class RelayTravelBookingIdRateLimitTarget(ApiRatelimiting.RateLimitService.RateLimitServiceClient client) : IRelayTravelBookingIdSink
{
    public async Task ForwardAsync(RelayTravelBookingIdRecord record, CancellationToken token)
    {
        await client.GetRateLimitAsync(new ApiRatelimiting.GetRateLimitRequest { RateLimitId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.recruiting.v1.JobOpeningCalculationService.</summary>
internal sealed class RelayTravelBookingIdJobOpeningCalculationSink : IRelayTravelBookingIdSink
{
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _client;

    public RelayTravelBookingIdJobOpeningCalculationSink(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(RelayTravelBookingIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.ExplainJobOpeningCalculationRequest { Reason = record.Status }, token, _client.ExplainJobOpeningCalculationAsync);
    }
}

/// <summary>Hands records to acme.ticketing.v1.TicketService.</summary>
internal sealed class RelayTravelBookingIdTicketWriter(ApiTicketing.TicketService.TicketServiceClient client) : IRelayTravelBookingIdSink
{
    public async Task ForwardAsync(RelayTravelBookingIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTicketing.GetTicketRequest { TicketId = record.Status }, token, client.GetTicketAsync);
    }
}

/// <summary>Reads acme.travel.v1.TravelBookingApprovalService/SubmitTravelBooking and hands what it keeps to the sink of its kind.</summary>
internal static class RelayTravelBookingId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient(channel);
        var response = await source.SubmitTravelBookingAsync(new ApiTravel.SubmitTravelBookingRequest() { Reason = "primary-relay-travel-booking-id" }, cancellationToken: token);
        var record = new RelayTravelBookingIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Status = response.Result.Status,
        };
        var sinks = new List<IRelayTravelBookingIdSink>
        {
            new RelayTravelBookingIdRateLimitTarget(new ApiRatelimiting.RateLimitService.RateLimitServiceClient(channel)),
            new RelayTravelBookingIdJobOpeningCalculationSink(new ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient(channel)),
            new RelayTravelBookingIdTicketWriter(new ApiTicketing.TicketService.TicketServiceClient(channel)),
        };
        await sinks[2].ForwardAsync(record, token);
    }
}
