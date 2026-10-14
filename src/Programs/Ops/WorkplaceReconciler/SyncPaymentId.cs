using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiPayments = Acme.Payments.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where SyncPaymentId hands each record.</summary>
internal interface ISyncPaymentIdDestination
{
    Task ApplyAsync(SyncPaymentIdRecord record, CancellationToken token);
}

/// <summary>What SyncPaymentId keeps of each QuotePayment response.</summary>
internal sealed class SyncPaymentIdRecord
{
    public string PaymentId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Hands records to acme.documents.v1.DocumentTemplateHistoryService.</summary>
internal sealed class SyncPaymentIdDocumentTemplateHistorySink : ISyncPaymentIdDestination
{
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _client;

    public SyncPaymentIdDocumentTemplateHistorySink(ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncPaymentIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.PruneDocumentTemplateVersionsRequest { DocumentTemplateId = record.Status, Reason = record.Number }, token, _client.PruneDocumentTemplateVersionsAsync);
    }
}

/// <summary>Hands records to acme.devices.v1.ComplianceScanLifecycleService.</summary>
internal sealed class SyncPaymentIdComplianceScanLifecycleWriter(ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient client) : ISyncPaymentIdDestination
{
    public async Task ApplyAsync(SyncPaymentIdRecord record, CancellationToken token)
    {
        await client.ExpireComplianceScanAsync(new ApiDevices.ExpireComplianceScanRequest { Details = new ApiDevices.ExpireComplianceScanRequest.Types.Details { ReasonCode = record.PaymentId }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.telematics.v1.TelemetryReadingQueryService.</summary>
internal sealed class SyncPaymentIdTelemetryReadingQueryWriter : ISyncPaymentIdDestination
{
    private readonly ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient _client;

    public SyncPaymentIdTelemetryReadingQueryWriter(ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncPaymentIdRecord record, CancellationToken token)
    {
        await _client.PeekTelemetryReadingAsync(new ApiTelematics.PeekTelemetryReadingRequest { TelemetryReadingId = record.Number }, cancellationToken: token);
    }
}

/// <summary>Reads acme.payments.v1.PaymentCalculationService/QuotePayment and hands what it keeps to the sink of its kind.</summary>
internal static class SyncPaymentId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient(channel);
        var response = await source.QuotePaymentAsync(new ApiPayments.QuotePaymentRequest() { PaymentId = "live-sync-payment-id" }, cancellationToken: token);
        var record = new SyncPaymentIdRecord
        {
            PaymentId = response.Payment.PaymentId,
            Status = Text.Normalize(response.Result.Status),
            Number = response.Payment.Number,
        };
        var sinks = new Dictionary<string, ISyncPaymentIdDestination>
        {
            ["document-template-history"] = new SyncPaymentIdDocumentTemplateHistorySink(new ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient(channel)),
            ["compliance-scan-lifecycle"] = new SyncPaymentIdComplianceScanLifecycleWriter(new ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient(channel)),
            ["telemetry-reading-query"] = new SyncPaymentIdTelemetryReadingQueryWriter(new ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient(channel)),
        };
        await RouteSyncPaymentIdAsync("document-template-history", record, sinks, token);
    }

    private static async Task RouteSyncPaymentIdAsync(string kind, SyncPaymentIdRecord record, IReadOnlyDictionary<string, ISyncPaymentIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
