using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where SyncTravelPolicyWorkflowsStatus hands each record.</summary>
internal interface ISyncTravelPolicyWorkflowsStatusDestination
{
    Task WriteAsync(SyncTravelPolicyWorkflowsStatusRecord record, CancellationToken token);
}

/// <summary>What SyncTravelPolicyWorkflowsStatus keeps of each ListTravelPolicyWorkflows response.</summary>
internal sealed class SyncTravelPolicyWorkflowsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Passes records to acme.featureflags.v1.FeatureFlagService.</summary>
internal sealed class SyncTravelPolicyWorkflowsStatusFeatureFlagWriter : ISyncTravelPolicyWorkflowsStatusDestination
{
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _client;

    public SyncTravelPolicyWorkflowsStatusFeatureFlagWriter(ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient client) => _client = client;

    public async Task WriteAsync(SyncTravelPolicyWorkflowsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFeatureflags.ListFeatureFlagsRequest { Filter = new ApiFeatureflags.ListFeatureFlagsRequest.Types.Filter { Query = record.CorrelationId } }, token, _client.ListFeatureFlagsAsync);
    }
}

/// <summary>Passes records to acme.travel.v1.ItineraryReportingService.</summary>
internal sealed class SyncTravelPolicyWorkflowsStatusItineraryReportingTarget(ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient client) : ISyncTravelPolicyWorkflowsStatusDestination
{
    public async Task WriteAsync(SyncTravelPolicyWorkflowsStatusRecord record, CancellationToken token)
    {
        await client.ListItineraryReportsAsync(new ApiTravel.ListItineraryReportsRequest { Filter = new ApiTravel.ListItineraryReportsRequest.Types.Filter { Region = record.NextPageToken, Query = record.Status }, OrderBy = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TravelPolicyWorkflowService/ListTravelPolicyWorkflows and hands what it keeps to the sink of its kind.</summary>
internal static class SyncTravelPolicyWorkflowsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient(channel);
        var found = await source.ListTravelPolicyWorkflowsAsync(new ApiTravel.ListTravelPolicyWorkflowsRequest() { OrderBy = "main-sync-travel-policy-workflows-status" }, cancellationToken: token);
        var record = new SyncTravelPolicyWorkflowsStatusRecord
        {
            Status = found.Result.Status,
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            NextPageToken = found.PageInfo.NextPageToken,
        };
        var sinks = new Dictionary<string, ISyncTravelPolicyWorkflowsStatusDestination>
        {
            ["feature-flag"] = new SyncTravelPolicyWorkflowsStatusFeatureFlagWriter(new ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient(channel)),
            ["itinerary-reporting"] = new SyncTravelPolicyWorkflowsStatusItineraryReportingTarget(new ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient(channel)),
        };
        await RouteSyncTravelPolicyWorkflowsStatusAsync("feature-flag", record, sinks, token);
    }

    private static async Task RouteSyncTravelPolicyWorkflowsStatusAsync(string kind, SyncTravelPolicyWorkflowsStatusRecord record, IReadOnlyDictionary<string, ISyncTravelPolicyWorkflowsStatusDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
