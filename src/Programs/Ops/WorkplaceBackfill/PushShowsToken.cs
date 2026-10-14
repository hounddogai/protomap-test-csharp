using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiShows = Acme.Shows.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where PushShowsToken hands each record.</summary>
internal interface IPushShowsTokenTarget
{
    Task SendAsync(PushShowsTokenRecord record, CancellationToken token);
}

/// <summary>What PushShowsToken keeps of each ResolveShows response.</summary>
internal sealed class PushShowsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.scheduling.v1.AvailabilityWindowApprovalService.</summary>
internal sealed class PushShowsTokenAvailabilityWindowApprovalWriter : IPushShowsTokenTarget
{
    private readonly ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient _client;

    public PushShowsTokenAvailabilityWindowApprovalWriter(ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient client) => _client = client;

    public async Task SendAsync(PushShowsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.SubmitAvailabilityWindowRequest { Details = new ApiScheduling.SubmitAvailabilityWindowRequest.Types.Details { ApproverEmail = record.CorrelationId }, AvailabilityWindowId = record.NextPageToken }, token, _client.SubmitAvailabilityWindowAsync);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelBookingCalculationService.</summary>
internal sealed class PushShowsTokenTravelBookingCalculationRelay(ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient client) : IPushShowsTokenTarget
{
    public async Task SendAsync(PushShowsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.GetTravelBookingRatesRequest { TravelBookingId = record.CorrelationId }, token, client.GetTravelBookingRatesAsync);
    }
}

/// <summary>Hands records to acme.corporateevents.v1.RegistrationWorkflowService.</summary>
internal sealed class PushShowsTokenRegistrationWorkflowSink(ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient client) : IPushShowsTokenTarget
{
    public async Task SendAsync(PushShowsTokenRecord record, CancellationToken token)
    {
        await client.StartRegistrationWorkflowAsync(new ApiCorporateevents.StartRegistrationWorkflowRequest { Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.shows.v1.ShowQueryService/ResolveShows and hands what it keeps to the sink of its kind.</summary>
internal static class PushShowsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiShows.ShowQueryService.ShowQueryServiceClient(channel);
        var response = await source.ResolveShowsAsync(new ApiShows.ResolveShowsRequest(), cancellationToken: token);
        var record = new PushShowsTokenRecord
        {
            NextPageToken = response.PageInfo.NextPageToken,
            CorrelationId = response.Result.CorrelationId,
            Status = response.Result.Status,
        };
        var sinks = new Dictionary<string, IPushShowsTokenTarget>
        {
            ["availability-window-approval"] = new PushShowsTokenAvailabilityWindowApprovalWriter(new ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient(channel)),
            ["travel-booking-calculation"] = new PushShowsTokenTravelBookingCalculationRelay(new ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient(channel)),
            ["registration-workflow"] = new PushShowsTokenRegistrationWorkflowSink(new ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "availability-window-approval")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
