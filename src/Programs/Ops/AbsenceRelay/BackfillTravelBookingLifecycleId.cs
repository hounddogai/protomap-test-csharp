using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where BackfillTravelBookingLifecycleId hands each record.</summary>
internal interface IBackfillTravelBookingLifecycleIdTarget
{
    Task DeliverAsync(BackfillTravelBookingLifecycleIdRecord record, CancellationToken token);
}

/// <summary>What BackfillTravelBookingLifecycleId keeps of each GetTravelBookingLifecycle response.</summary>
internal sealed class BackfillTravelBookingLifecycleIdRecord
{
    public string TravelBookingId { get; init; } = "";
    public string Number { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Passes records to acme.travel.v1.TravelBookingLifecycleService.</summary>
internal sealed class BackfillTravelBookingLifecycleIdTravelBookingLifecycleTarget(ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient client) : IBackfillTravelBookingLifecycleIdTarget
{
    public async Task DeliverAsync(BackfillTravelBookingLifecycleIdRecord record, CancellationToken token)
    {
        await client.SuspendTravelBookingAsync(new ApiTravel.SuspendTravelBookingRequest { TravelBookingId = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.deliveries.v1.DeliveryQueryService.</summary>
internal sealed class BackfillTravelBookingLifecycleIdDeliveryQueryTarget(ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient client) : IBackfillTravelBookingLifecycleIdTarget
{
    public async Task DeliverAsync(BackfillTravelBookingLifecycleIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDeliveries.CountDeliveriesRequest { Filter = new ApiDeliveries.CountDeliveriesRequest.Types.Filter { Region = record.CorrelationId, OwnerId = record.Notes }, Page = new ApiCommon.PageRequest { PageToken = record.Number } }, token, client.CountDeliveriesAsync);
    }
}

/// <summary>Sends records to acme.learning.v1.CertificationApprovalService.</summary>
internal sealed class BackfillTravelBookingLifecycleIdCertificationApprovalTarget : IBackfillTravelBookingLifecycleIdTarget
{
    private readonly ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient _client;

    public BackfillTravelBookingLifecycleIdCertificationApprovalTarget(ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(BackfillTravelBookingLifecycleIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.DelegateCertificationApprovalRequest { Reason = record.TravelBookingId, Certification = new ApiLearning.Certification { Number = record.Number }, Details = new ApiLearning.DelegateCertificationApprovalRequest.Types.Details { ApproverEmail = record.CorrelationId } }, token, _client.DelegateCertificationApprovalAsync);
    }
}

/// <summary>Reads acme.travel.v1.TravelBookingLifecycleService/GetTravelBookingLifecycle and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillTravelBookingLifecycleId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient(channel);
        var reply = await source.GetTravelBookingLifecycleAsync(new ApiTravel.GetTravelBookingLifecycleRequest() { TravelBookingId = "primary-backfill-travel-booking-lifecycle-id" }, cancellationToken: token);
        var record = new BackfillTravelBookingLifecycleIdRecord
        {
            TravelBookingId = reply.TravelBooking.TravelBookingId,
            Number = reply.TravelBooking.Number,
            CorrelationId = reply.Result.CorrelationId,
            Notes = Text.Normalize(reply.TravelBooking.Notes),
        };
        await ActOnBackfillTravelBookingLifecycleIdAsync("certification-approval", record, channel, token);
    }

    private static async Task ActOnBackfillTravelBookingLifecycleIdAsync(string kind, BackfillTravelBookingLifecycleIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["travel-booking-lifecycle"] = () => new BackfillTravelBookingLifecycleIdTravelBookingLifecycleTarget(new ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient(channel)).DeliverAsync(record, token),
            ["delivery-query"] = () => new BackfillTravelBookingLifecycleIdDeliveryQueryTarget(new ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient(channel)).DeliverAsync(record, token),
            ["certification-approval"] = () => new BackfillTravelBookingLifecycleIdCertificationApprovalTarget(new ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
