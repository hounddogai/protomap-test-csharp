using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where PushFeatureViewId hands each record.</summary>
internal interface IPushFeatureViewIdDestination
{
    Task ApplyAsync(PushFeatureViewIdRecord record, CancellationToken token);
}

/// <summary>What PushFeatureViewId keeps of each QuoteFeatureView response.</summary>
internal sealed class PushFeatureViewIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Hands records to acme.webhooks.v1.WebhookEndpointQueryService.</summary>
internal sealed class PushFeatureViewIdWebhookEndpointQuerySink : IPushFeatureViewIdDestination
{
    private readonly ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient _client;

    public PushFeatureViewIdWebhookEndpointQuerySink(ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(PushFeatureViewIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWebhooks.SuggestWebhookEndpointsRequest { Filter = new ApiWebhooks.SuggestWebhookEndpointsRequest.Types.Filter { Region = record.CorrelationId } }, token, _client.SuggestWebhookEndpointsAsync);
    }
}

/// <summary>Hands records to acme.travel.v1.TripQueryService.</summary>
internal sealed class PushFeatureViewIdTripQueryRelay(ApiTravel.TripQueryService.TripQueryServiceClient client) : IPushFeatureViewIdDestination
{
    public async Task ApplyAsync(PushFeatureViewIdRecord record, CancellationToken token)
    {
        await client.CountTripsAsync(new ApiTravel.CountTripsRequest { Filter = new ApiTravel.CountTripsRequest.Types.Filter { Region = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.featurestore.v1.FeatureViewCalculationService/QuoteFeatureView and hands what it keeps to the sink of its kind.</summary>
internal static class PushFeatureViewId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(channel);
        var found = await source.QuoteFeatureViewAsync(new ApiFeaturestore.QuoteFeatureViewRequest() { FeatureViewId = "main-push-feature-view-id" }, cancellationToken: token);
        var record = new PushFeatureViewIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Name = found.FeatureView.Name,
        };
        await ActOnPushFeatureViewIdAsync("trip-query", record, channel, token);
    }

    private static async Task ActOnPushFeatureViewIdAsync(string kind, PushFeatureViewIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["webhook-endpoint-query"] = () => new PushFeatureViewIdWebhookEndpointQuerySink(new ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient(channel)).ApplyAsync(record, token),
            ["trip-query"] = () => new PushFeatureViewIdTripQueryRelay(new ApiTravel.TripQueryService.TripQueryServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
