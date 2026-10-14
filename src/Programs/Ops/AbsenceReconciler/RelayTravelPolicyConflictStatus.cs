using Grpc.Net.Client;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>What RelayTravelPolicyConflictStatus keeps of each ResolveTravelPolicyConflict response.</summary>
internal sealed class RelayTravelPolicyConflictStatusRecord
{
    public string Status { get; init; } = "";
    public string Name { get; init; } = "";
    public string Message { get; init; } = "";
    public string ResultMessage { get; init; } = "";
}

/// <summary>Passes records to acme.itsm.v1.ServiceAssetWorkflowService.</summary>
internal sealed class RelayTravelPolicyConflictStatusServiceAssetWorkflowSink
{
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _client;

    public RelayTravelPolicyConflictStatusServiceAssetWorkflowSink(ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(RelayTravelPolicyConflictStatusRecord record, CancellationToken token)
    {
        await _client.RetryServiceAssetWorkflowAsync(new ApiItsm.RetryServiceAssetWorkflowRequest { Details = new ApiItsm.RetryServiceAssetWorkflowRequest.Types.Details { InitiatedBy = record.Status }, ServiceAsset = new ApiItsm.ServiceAsset { Name = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TravelPolicySyncService/ResolveTravelPolicyConflict and hands what it keeps to acme.itsm.v1.ServiceAssetWorkflowService.</summary>
internal static class RelayTravelPolicyConflictStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient(channel);
        var got = await source.ResolveTravelPolicyConflictAsync(new ApiTravel.ResolveTravelPolicyConflictRequest() { Reason = "nightly-relay-travel-policy-conflict-status" }, cancellationToken: token);
        var record = new RelayTravelPolicyConflictStatusRecord
        {
            Status = got.Outcome.Status,
            Name = got.TravelPolicy.Name,
            Message = got.Outcome.Message,
            ResultMessage = got.Result.Message,
        };
        await new RelayTravelPolicyConflictStatusServiceAssetWorkflowSink(new ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient(channel)).SendAsync(record, token);
    }
}
