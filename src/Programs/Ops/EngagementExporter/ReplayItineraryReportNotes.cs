using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCatalog = Acme.Catalog.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where ReplayItineraryReportNotes hands each record.</summary>
internal interface IReplayItineraryReportNotesSink
{
    Task ApplyAsync(ReplayItineraryReportNotesRecord record, CancellationToken token);
}

/// <summary>What ReplayItineraryReportNotes keeps of each FetchItineraryReport response.</summary>
internal sealed class ReplayItineraryReportNotesRecord
{
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.catalog.v1.ProductQueryService.</summary>
internal sealed class ReplayItineraryReportNotesProductQuerySink : IReplayItineraryReportNotesSink
{
    private readonly ApiCatalog.ProductQueryService.ProductQueryServiceClient _client;

    public ReplayItineraryReportNotesProductQuerySink(ApiCatalog.ProductQueryService.ProductQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(ReplayItineraryReportNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCatalog.CountProductsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Notes }, Filter = new ApiCatalog.CountProductsRequest.Types.Filter { Region = record.Status } }, token, _client.CountProductsAsync);
    }
}

/// <summary>Hands records to acme.recommendations.v1.RecommendationSetQueryService.</summary>
internal sealed class ReplayItineraryReportNotesRecommendationSetQueryTarget(ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient client) : IReplayItineraryReportNotesSink
{
    public async Task ApplyAsync(ReplayItineraryReportNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecommendations.BrowseRecommendationSetsRequest { OrderBy = record.Status, Page = new ApiCommon.PageRequest { PageToken = record.Notes } }, token, client.BrowseRecommendationSetsAsync);
    }
}

/// <summary>Sends records to acme.customers.v1.CustomerService.</summary>
internal sealed class ReplayItineraryReportNotesCustomerTarget(ApiCustomers.CustomerService.CustomerServiceClient client) : IReplayItineraryReportNotesSink
{
    public async Task ApplyAsync(ReplayItineraryReportNotesRecord record, CancellationToken token)
    {
        await client.ListCustomersAsync(new ApiCustomers.ListCustomersRequest { Filter = new ApiCustomers.ListCustomersRequest.Types.Filter { Query = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplayItineraryReportNotesLogOnly : IReplayItineraryReportNotesSink
{
    public Task ApplyAsync(ReplayItineraryReportNotesRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Notes}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.travel.v1.ItineraryReportingService/FetchItineraryReport and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayItineraryReportNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient(channel);
        var found = await source.FetchItineraryReportAsync(new ApiTravel.FetchItineraryReportRequest() { ItineraryId = "default-replay-itinerary-report-notes" }, cancellationToken: token);
        var record = new ReplayItineraryReportNotesRecord
        {
            Notes = found.Itinerary.Notes,
            Status = found.Result.Status,
        };
        await DispatchReplayItineraryReportNotesAsync("product-query", record, channel, token);
    }

    private static async Task DispatchReplayItineraryReportNotesAsync(string kind, ReplayItineraryReportNotesRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<ReplayItineraryReportNotesRecord, CancellationToken, Task> write = new ReplayItineraryReportNotesLogOnly().ApplyAsync;
        switch (kind)
        {
            case "product-query":
                write = new ReplayItineraryReportNotesProductQuerySink(new ApiCatalog.ProductQueryService.ProductQueryServiceClient(channel)).ApplyAsync;
                break;
            case "recommendation-set-query":
                write = new ReplayItineraryReportNotesRecommendationSetQueryTarget(new ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient(channel)).ApplyAsync;
                break;
            case "customer":
                write = new ReplayItineraryReportNotesCustomerTarget(new ApiCustomers.CustomerService.CustomerServiceClient(channel)).ApplyAsync;
                break;
        }
        await write(record, token);
    }
}
