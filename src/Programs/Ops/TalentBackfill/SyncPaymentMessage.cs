using Grpc.Net.Client;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiPayments = Acme.Payments.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where SyncPaymentMessage hands each record.</summary>
internal interface ISyncPaymentMessageSink
{
    Task ForwardAsync(SyncPaymentMessageRecord record, CancellationToken token);
}

/// <summary>What SyncPaymentMessage keeps of each SimulatePayment response.</summary>
internal sealed class SyncPaymentMessageRecord
{
    public string Message { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Writes records to acme.concessions.v1.ConcessionStandQueryService.</summary>
internal sealed class SyncPaymentMessageConcessionStandQueryRelay : ISyncPaymentMessageSink
{
    private readonly ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient _client;

    public SyncPaymentMessageConcessionStandQueryRelay(ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncPaymentMessageRecord record, CancellationToken token)
    {
        await _client.SummarizeConcessionStandsAsync(new ApiConcessions.SummarizeConcessionStandsRequest { Reason = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.boxoffice.v1.BoxOfficeSaleQueryService.</summary>
internal sealed class SyncPaymentMessageBoxOfficeSaleQueryTarget(ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient client) : ISyncPaymentMessageSink
{
    public async Task ForwardAsync(SyncPaymentMessageRecord record, CancellationToken token)
    {
        await client.PeekBoxOfficeSaleAsync(new ApiBoxoffice.PeekBoxOfficeSaleRequest { BoxOfficeSaleId = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.ledger.v1.LedgerAccountValidationService.</summary>
internal sealed class SyncPaymentMessageLedgerAccountValidationSink : ISyncPaymentMessageSink
{
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _client;

    public SyncPaymentMessageLedgerAccountValidationSink(ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncPaymentMessageRecord record, CancellationToken token)
    {
        await _client.PreviewLedgerAccountAsync(new ApiLedger.PreviewLedgerAccountRequest { LedgerAccountId = record.CurrencyCode, Details = new ApiLedger.PreviewLedgerAccountRequest.Types.Details { Locale = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.payments.v1.PaymentCalculationService/SimulatePayment and hands what it keeps to the sink of its kind.</summary>
internal static class SyncPaymentMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient(channel);
        var response = await source.SimulatePaymentAsync(new ApiPayments.SimulatePaymentRequest() { Reason = "live-sync-payment-message" }, cancellationToken: token);
        var record = new SyncPaymentMessageRecord
        {
            Message = response.Result.Message,
            CurrencyCode = response.Breakdown.CurrencyCode,
        };
        var sinks = new Dictionary<string, ISyncPaymentMessageSink>
        {
            ["concession-stand-query"] = new SyncPaymentMessageConcessionStandQueryRelay(new ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient(channel)),
            ["box-office-sale-query"] = new SyncPaymentMessageBoxOfficeSaleQueryTarget(new ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient(channel)),
            ["ledger-account-validation"] = new SyncPaymentMessageLedgerAccountValidationSink(new ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "concession-stand-query")
                continue;
            await sink.ForwardAsync(record, token);
        }
    }
}
