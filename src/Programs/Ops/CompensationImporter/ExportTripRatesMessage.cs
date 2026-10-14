using Grpc.Net.Client;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where ExportTripRatesMessage hands each record.</summary>
internal interface IExportTripRatesMessageTarget
{
    Task SendAsync(ExportTripRatesMessageRecord record, CancellationToken token);
}

/// <summary>What ExportTripRatesMessage keeps of each GetTripRates response.</summary>
internal sealed class ExportTripRatesMessageRecord
{
    public string Message { get; init; } = "";
    public string Number { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.documents.v1.DocumentFolderExportService.</summary>
internal sealed class ExportTripRatesMessageDocumentFolderExportRelay : IExportTripRatesMessageTarget
{
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _client;

    public ExportTripRatesMessageDocumentFolderExportRelay(ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient client) => _client = client;

    public async Task SendAsync(ExportTripRatesMessageRecord record, CancellationToken token)
    {
        await _client.ListDocumentFolderExportsAsync(new ApiDocuments.ListDocumentFolderExportsRequest { Filter = new ApiDocuments.ListDocumentFolderExportsRequest.Types.Filter { Region = record.Number, Query = record.CorrelationId }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.itsm.v1.ServiceAssetApprovalService.</summary>
internal sealed class ExportTripRatesMessageServiceAssetApprovalSink(ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient client) : IExportTripRatesMessageTarget
{
    public async Task SendAsync(ExportTripRatesMessageRecord record, CancellationToken token)
    {
        await client.DelegateServiceAssetApprovalAsync(new ApiItsm.DelegateServiceAssetApprovalRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.OfferSyncService.</summary>
internal sealed class ExportTripRatesMessageOfferSyncRelay : IExportTripRatesMessageTarget
{
    private readonly ApiRecruiting.OfferSyncService.OfferSyncServiceClient _client;

    public ExportTripRatesMessageOfferSyncRelay(ApiRecruiting.OfferSyncService.OfferSyncServiceClient client) => _client = client;

    public async Task SendAsync(ExportTripRatesMessageRecord record, CancellationToken token)
    {
        await _client.SyncOffersAsync(new ApiRecruiting.SyncOffersRequest { Details = new ApiRecruiting.SyncOffersRequest.Types.Details { Resolution = record.CorrelationId, Cursor = record.Number }, Offer = new ApiRecruiting.Offer { OfferId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TripCalculationService/GetTripRates and hands what it keeps to the sink of its kind.</summary>
internal static class ExportTripRatesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripCalculationService.TripCalculationServiceClient(channel);
        var got = await source.GetTripRatesAsync(new ApiTravel.GetTripRatesRequest() { TripId = "main-export-trip-rates-message" }, cancellationToken: token);
        var record = new ExportTripRatesMessageRecord
        {
            Message = got.Result.Message,
            Number = got.Trip.Number,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IExportTripRatesMessageTarget>
        {
            ["document-folder-export"] = new ExportTripRatesMessageDocumentFolderExportRelay(new ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient(channel)),
            ["asset-approval"] = new ExportTripRatesMessageServiceAssetApprovalSink(new ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient(channel)),
            ["offer-sync"] = new ExportTripRatesMessageOfferSyncRelay(new ApiRecruiting.OfferSyncService.OfferSyncServiceClient(channel)),
        };
        await RouteExportTripRatesMessageAsync("document-folder-export", record, sinks, token);
    }

    private static async Task RouteExportTripRatesMessageAsync(string kind, ExportTripRatesMessageRecord record, IReadOnlyDictionary<string, IExportTripRatesMessageTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
