using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLedger = Acme.Ledger.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where BackfillInvoiceBatchCode hands each record.</summary>
internal interface IBackfillInvoiceBatchCodeSink
{
    Task WriteAsync(BackfillInvoiceBatchCodeRecord record, CancellationToken token);
}

/// <summary>What BackfillInvoiceBatchCode keeps of each QuoteInvoiceBatch response.</summary>
internal sealed class BackfillInvoiceBatchCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string Status { get; init; } = "";
    public string InvoiceBatchId { get; init; } = "";
}

/// <summary>Passes records to acme.helpdesk.v1.TicketValidationService.</summary>
internal sealed class BackfillInvoiceBatchCodeTicketValidationSink(ApiHelpdesk.TicketValidationService.TicketValidationServiceClient client) : IBackfillInvoiceBatchCodeSink
{
    public async Task WriteAsync(BackfillInvoiceBatchCodeRecord record, CancellationToken token)
    {
        await client.ValidateTicketAsync(new ApiHelpdesk.ValidateTicketRequest { Details = new ApiHelpdesk.ValidateTicketRequest.Types.Details { Locale = record.CurrencyCode }, Ticket = new ApiHelpdesk.Ticket { TicketId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentSyncService.</summary>
internal sealed class BackfillInvoiceBatchCodeDocumentSyncRelay : IBackfillInvoiceBatchCodeSink
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _client;

    public BackfillInvoiceBatchCodeDocumentSyncRelay(ApiDocuments.DocumentSyncService.DocumentSyncServiceClient client) => _client = client;

    public async Task WriteAsync(BackfillInvoiceBatchCodeRecord record, CancellationToken token)
    {
        await _client.ReconcileDocumentsAsync(new ApiDocuments.ReconcileDocumentsRequest { DocumentId = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.ledger.v1.LedgerAccountQueryService.</summary>
internal sealed class BackfillInvoiceBatchCodeLedgerAccountQuerySink(ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient client) : IBackfillInvoiceBatchCodeSink
{
    public async Task WriteAsync(BackfillInvoiceBatchCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLedger.LookupLedgerAccountRequest { LedgerAccountId = record.CurrencyCode }, token, client.LookupLedgerAccountAsync);
    }
}

/// <summary>Reads acme.invoicing.v1.InvoiceBatchCalculationService/QuoteInvoiceBatch and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillInvoiceBatchCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(channel);
        var response = await source.QuoteInvoiceBatchAsync(new ApiInvoicing.QuoteInvoiceBatchRequest() { Reason = "main-backfill-invoice-batch-code" }, cancellationToken: token);
        var record = new BackfillInvoiceBatchCodeRecord
        {
            CurrencyCode = response.Breakdown.CurrencyCode,
            Status = response.Result.Status,
            InvoiceBatchId = response.InvoiceBatch.InvoiceBatchId,
        };
        var sinks = new Dictionary<string, IBackfillInvoiceBatchCodeSink>
        {
            ["ticket-validation"] = new BackfillInvoiceBatchCodeTicketValidationSink(new ApiHelpdesk.TicketValidationService.TicketValidationServiceClient(channel)),
            ["document-sync"] = new BackfillInvoiceBatchCodeDocumentSyncRelay(new ApiDocuments.DocumentSyncService.DocumentSyncServiceClient(channel)),
            ["ledger-account-query"] = new BackfillInvoiceBatchCodeLedgerAccountQuerySink(new ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "document-sync")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
