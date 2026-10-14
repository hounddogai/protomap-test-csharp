using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where SettleDeliveriesMessage hands each record.</summary>
internal interface ISettleDeliveriesMessageWriter
{
    Task ApplyAsync(SettleDeliveriesMessageRecord record, CancellationToken token);
}

/// <summary>What SettleDeliveriesMessage keeps of each SummarizeDeliveries response.</summary>
internal sealed class SettleDeliveriesMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string ReportId { get; init; } = "";
}

/// <summary>Writes records to acme.licensing.v1.LicenseAssignmentReportingService.</summary>
internal sealed class SettleDeliveriesMessageLicenseAssignmentReportingSink(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : ISettleDeliveriesMessageWriter
{
    public async Task ApplyAsync(SettleDeliveriesMessageRecord record, CancellationToken token)
    {
        await client.FetchLicenseAssignmentReportAsync(new ApiLicensing.FetchLicenseAssignmentReportRequest { LicenseAssignmentId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentValidationService.</summary>
internal sealed class SettleDeliveriesMessageDocumentValidationTarget : ISettleDeliveriesMessageWriter
{
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _client;

    public SettleDeliveriesMessageDocumentValidationTarget(ApiDocuments.DocumentValidationService.DocumentValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleDeliveriesMessageRecord record, CancellationToken token)
    {
        await _client.NormalizeDocumentAsync(new ApiDocuments.NormalizeDocumentRequest { Document = new ApiDocuments.Document { DocumentId = record.ReportId }, Details = new ApiDocuments.NormalizeDocumentRequest.Types.Details { Locale = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.corporateevents.v1.VenueSyncService.</summary>
internal sealed class SettleDeliveriesMessageVenueSyncTarget : ISettleDeliveriesMessageWriter
{
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _client;

    public SettleDeliveriesMessageVenueSyncTarget(ApiCorporateevents.VenueSyncService.VenueSyncServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleDeliveriesMessageRecord record, CancellationToken token)
    {
        await _client.ResolveVenueConflictAsync(new ApiCorporateevents.ResolveVenueConflictRequest { Venue = new ApiCorporateevents.Venue { Code = record.ReportId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SettleDeliveriesMessageLogOnly : ISettleDeliveriesMessageWriter
{
    public Task ApplyAsync(SettleDeliveriesMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.deliveries.v1.DeliveryQueryService/SummarizeDeliveries and hands what it keeps to the sink of its kind.</summary>
internal static class SettleDeliveriesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient(channel);
        var response = await source.SummarizeDeliveriesAsync(new ApiDeliveries.SummarizeDeliveriesRequest() { Reason = "main-settle-deliveries-message" }, cancellationToken: token);
        var record = new SettleDeliveriesMessageRecord
        {
            Message = response.Result.Message,
            CorrelationId = response.Result.CorrelationId,
            ReportId = Text.Normalize(response.ReportId),
        };
        await DeliverSettleDeliveriesMessageAsync("venue-sync", record, channel, token);
    }

    private static async Task DeliverSettleDeliveriesMessageAsync(string kind, SettleDeliveriesMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISettleDeliveriesMessageWriter sink = new SettleDeliveriesMessageLogOnly();
        switch (kind)
        {
            case "license-assignment-reporting":
                sink = new SettleDeliveriesMessageLicenseAssignmentReportingSink(new ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient(channel));
                break;
            case "document-validation":
                sink = new SettleDeliveriesMessageDocumentValidationTarget(new ApiDocuments.DocumentValidationService.DocumentValidationServiceClient(channel));
                break;
            case "venue-sync":
                sink = new SettleDeliveriesMessageVenueSyncTarget(new ApiCorporateevents.VenueSyncService.VenueSyncServiceClient(channel));
                break;
        }
        await sink.ApplyAsync(record, token);
    }
}
