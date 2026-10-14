using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where BackfillDocumentTemplateWorkflowsToken hands each record.</summary>
internal interface IBackfillDocumentTemplateWorkflowsTokenSink
{
    Task ForwardAsync(BackfillDocumentTemplateWorkflowsTokenRecord record, CancellationToken token);
}

/// <summary>What BackfillDocumentTemplateWorkflowsToken keeps of each ListDocumentTemplateWorkflows response.</summary>
internal sealed class BackfillDocumentTemplateWorkflowsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.travel.v1.ItineraryCalculationService.</summary>
internal sealed class BackfillDocumentTemplateWorkflowsTokenItineraryCalculationWriter : IBackfillDocumentTemplateWorkflowsTokenSink
{
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _client;

    public BackfillDocumentTemplateWorkflowsTokenItineraryCalculationWriter(ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillDocumentTemplateWorkflowsTokenRecord record, CancellationToken token)
    {
        await _client.CalculateItineraryAsync(new ApiTravel.CalculateItineraryRequest { Itinerary = new ApiTravel.Itinerary { ItineraryId = record.Status, Number = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.TripCalculationService.</summary>
internal sealed class BackfillDocumentTemplateWorkflowsTokenTripCalculationWriter : IBackfillDocumentTemplateWorkflowsTokenSink
{
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _client;

    public BackfillDocumentTemplateWorkflowsTokenTripCalculationWriter(ApiTravel.TripCalculationService.TripCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillDocumentTemplateWorkflowsTokenRecord record, CancellationToken token)
    {
        await _client.CompareTripOptionsAsync(new ApiTravel.CompareTripOptionsRequest { Trip = new ApiTravel.Trip { Notes = record.NextPageToken, Number = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.travel.v1.TripWorkflowService.</summary>
internal sealed class BackfillDocumentTemplateWorkflowsTokenTripWorkflowWriter : IBackfillDocumentTemplateWorkflowsTokenSink
{
    private readonly ApiTravel.TripWorkflowService.TripWorkflowServiceClient _client;

    public BackfillDocumentTemplateWorkflowsTokenTripWorkflowWriter(ApiTravel.TripWorkflowService.TripWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillDocumentTemplateWorkflowsTokenRecord record, CancellationToken token)
    {
        await _client.AdvanceTripWorkflowAsync(new ApiTravel.AdvanceTripWorkflowRequest { TripId = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillDocumentTemplateWorkflowsTokenLogOnly : IBackfillDocumentTemplateWorkflowsTokenSink
{
    public Task ForwardAsync(BackfillDocumentTemplateWorkflowsTokenRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.NextPageToken}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.DocumentTemplateWorkflowService/ListDocumentTemplateWorkflows and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillDocumentTemplateWorkflowsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient(channel);
        var found = await source.ListDocumentTemplateWorkflowsAsync(new ApiDocuments.ListDocumentTemplateWorkflowsRequest() { OrderBy = "nightly-backfill-document-template-workflows-token" }, cancellationToken: token);
        var record = new BackfillDocumentTemplateWorkflowsTokenRecord
        {
            NextPageToken = Text.Normalize(found.PageInfo.NextPageToken),
            Status = Text.Normalize(found.Result.Status),
        };
        await DispatchBackfillDocumentTemplateWorkflowsTokenAsync("trip-workflow", record, channel, token);
    }

    private static async Task DispatchBackfillDocumentTemplateWorkflowsTokenAsync(string kind, BackfillDocumentTemplateWorkflowsTokenRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<BackfillDocumentTemplateWorkflowsTokenRecord, CancellationToken, Task> write = new BackfillDocumentTemplateWorkflowsTokenLogOnly().ForwardAsync;
        switch (kind)
        {
            case "itinerary-calculation":
                write = new BackfillDocumentTemplateWorkflowsTokenItineraryCalculationWriter(new ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient(channel)).ForwardAsync;
                break;
            case "trip-calculation":
                write = new BackfillDocumentTemplateWorkflowsTokenTripCalculationWriter(new ApiTravel.TripCalculationService.TripCalculationServiceClient(channel)).ForwardAsync;
                break;
            case "trip-workflow":
                write = new BackfillDocumentTemplateWorkflowsTokenTripWorkflowWriter(new ApiTravel.TripWorkflowService.TripWorkflowServiceClient(channel)).ForwardAsync;
                break;
        }
        await write(record, token);
    }
}
