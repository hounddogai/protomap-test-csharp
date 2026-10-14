using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCheckout = Acme.Checkout.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where MirrorTripsStatus hands each record.</summary>
internal interface IMirrorTripsStatusDestination
{
    Task ApplyAsync(MirrorTripsStatusRecord record, CancellationToken token);
}

/// <summary>What MirrorTripsStatus keeps of each ListTrips response.</summary>
internal sealed class MirrorTripsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.AvailabilityWindowWorkflowService.</summary>
internal sealed class MirrorTripsStatusAvailabilityWindowWorkflowTarget(ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient client) : IMirrorTripsStatusDestination
{
    public async Task ApplyAsync(MirrorTripsStatusRecord record, CancellationToken token)
    {
        await client.InspectAvailabilityWindowWorkflowAsync(new ApiScheduling.InspectAvailabilityWindowWorkflowRequest { AvailabilityWindowId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class MirrorTripsStatusCheckoutSessionRelay : IMirrorTripsStatusDestination
{
    private readonly ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient _client;

    public MirrorTripsStatusCheckoutSessionRelay(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorTripsStatusRecord record, CancellationToken token)
    {
        await _client.UpdateCheckoutSessionAsync(new ApiCheckout.UpdateCheckoutSessionRequest { CheckoutSession = new ApiCheckout.CheckoutSession { CheckoutSessionId = record.Status, Number = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TripService/ListTrips and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorTripsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripService.TripServiceClient(channel);
        var reply = await source.ListTripsAsync(new ApiTravel.ListTripsRequest() { OrderBy = "live-mirror-trips-status" }, cancellationToken: token);
        var record = new MirrorTripsStatusRecord
        {
            Status = Text.Normalize(reply.Result.Status),
            Message = reply.Result.Message,
            NextPageToken = reply.PageInfo.NextPageToken,
            CorrelationId = reply.Result.CorrelationId,
        };
        await ActOnMirrorTripsStatusAsync("availability-window-workflow", record, channel, token);
    }

    private static async Task ActOnMirrorTripsStatusAsync(string kind, MirrorTripsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["availability-window-workflow"] = () => new MirrorTripsStatusAvailabilityWindowWorkflowTarget(new ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient(channel)).ApplyAsync(record, token),
            ["checkout-session"] = () => new MirrorTripsStatusCheckoutSessionRelay(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
