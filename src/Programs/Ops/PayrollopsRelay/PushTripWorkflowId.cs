using Grpc.Net.Client;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>What PushTripWorkflowId keeps of each StartTripWorkflow response.</summary>
internal sealed class PushTripWorkflowIdRecord
{
    public string TripId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Sends records to acme.licensing.v1.EntitlementWorkflowService.</summary>
internal sealed class PushTripWorkflowIdEntitlementWorkflowWriter(ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient client)
{
    public async Task WriteAsync(PushTripWorkflowIdRecord record, CancellationToken token)
    {
        await client.AdvanceEntitlementWorkflowAsync(new ApiLicensing.AdvanceEntitlementWorkflowRequest { Entitlement = new ApiLicensing.Entitlement { Description = record.TripId, Name = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TripWorkflowService/StartTripWorkflow and hands what it keeps to acme.licensing.v1.EntitlementWorkflowService.</summary>
internal static class PushTripWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripWorkflowService.TripWorkflowServiceClient(channel);
        var found = await source.StartTripWorkflowAsync(new ApiTravel.StartTripWorkflowRequest() { Reason = "default-push-trip-workflow-id" }, cancellationToken: token);
        var record = new PushTripWorkflowIdRecord
        {
            TripId = found.Trip.TripId,
            CorrelationId = found.Result.CorrelationId,
            Number = found.Trip.Number,
        };
        await new PushTripWorkflowIdEntitlementWorkflowWriter(new ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient(channel)).WriteAsync(record, token);
    }
}
