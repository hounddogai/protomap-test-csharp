using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRouting = Acme.Routing.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where SyncRouteOptionsId hands each record.</summary>
internal interface ISyncRouteOptionsIdDestination
{
    Task PushAsync(SyncRouteOptionsIdRecord record, CancellationToken token);
}

/// <summary>What SyncRouteOptionsId keeps of each CompareRouteOptions response.</summary>
internal sealed class SyncRouteOptionsIdRecord
{
    public string RouteId { get; init; } = "";
    public string Number { get; init; } = "";
    public string Message { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Writes records to acme.ratelimiting.v1.RateLimitService.</summary>
internal sealed class SyncRouteOptionsIdRateLimitWriter(ApiRatelimiting.RateLimitService.RateLimitServiceClient client) : ISyncRouteOptionsIdDestination
{
    public async Task PushAsync(SyncRouteOptionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRatelimiting.SearchRateLimitsRequest { Filter = new ApiRatelimiting.SearchRateLimitsRequest.Types.Filter { Region = record.Notes }, Page = new ApiCommon.PageRequest { PageToken = record.Number } }, token, client.SearchRateLimitsAsync);
    }
}

/// <summary>Passes records to acme.documents.v1.DocumentTemplateNotificationService.</summary>
internal sealed class SyncRouteOptionsIdDocumentTemplateNotificationWriter(ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient client) : ISyncRouteOptionsIdDestination
{
    public async Task PushAsync(SyncRouteOptionsIdRecord record, CancellationToken token)
    {
        await client.NotifyDocumentTemplateWatchersAsync(new ApiDocuments.NotifyDocumentTemplateWatchersRequest { Reason = record.RouteId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.performance.v1.ReviewQueryService.</summary>
internal sealed class SyncRouteOptionsIdReviewQuerySink : ISyncRouteOptionsIdDestination
{
    private readonly ApiPerformance.ReviewQueryService.ReviewQueryServiceClient _client;

    public SyncRouteOptionsIdReviewQuerySink(ApiPerformance.ReviewQueryService.ReviewQueryServiceClient client) => _client = client;

    public async Task PushAsync(SyncRouteOptionsIdRecord record, CancellationToken token)
    {
        await _client.FindReviewsAsync(new ApiPerformance.FindReviewsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Number }, Filter = new ApiPerformance.FindReviewsRequest.Types.Filter { Region = record.Notes, Query = record.RouteId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncRouteOptionsIdLogOnly : ISyncRouteOptionsIdDestination
{
    public Task PushAsync(SyncRouteOptionsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.RouteId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.routing.v1.RouteCalculationService/CompareRouteOptions and hands what it keeps to the sink of its kind.</summary>
internal static class SyncRouteOptionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRouting.RouteCalculationService.RouteCalculationServiceClient(channel);
        var got = await source.CompareRouteOptionsAsync(new ApiRouting.CompareRouteOptionsRequest() { RouteId = "live-sync-route-options-id" }, cancellationToken: token);
        var record = new SyncRouteOptionsIdRecord
        {
            RouteId = got.Route.RouteId,
            Number = Text.Normalize(got.Route.Number),
            Message = got.Result.Message,
            Notes = got.Route.Notes,
        };
        await DispatchSyncRouteOptionsIdAsync("document-template-notification", record, channel, token);
    }

    private static async Task DispatchSyncRouteOptionsIdAsync(string kind, SyncRouteOptionsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SyncRouteOptionsIdRecord, CancellationToken, Task> write = new SyncRouteOptionsIdLogOnly().PushAsync;
        switch (kind)
        {
            case "rate-limit":
                write = new SyncRouteOptionsIdRateLimitWriter(new ApiRatelimiting.RateLimitService.RateLimitServiceClient(channel)).PushAsync;
                break;
            case "document-template-notification":
                write = new SyncRouteOptionsIdDocumentTemplateNotificationWriter(new ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient(channel)).PushAsync;
                break;
            case "review-query":
                write = new SyncRouteOptionsIdReviewQuerySink(new ApiPerformance.ReviewQueryService.ReviewQueryServiceClient(channel)).PushAsync;
                break;
        }
        await write(record, token);
    }
}
