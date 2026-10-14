using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where ReplayServiceAssetCode hands each record.</summary>
internal interface IReplayServiceAssetCodeDestination
{
    Task PushAsync(ReplayServiceAssetCodeRecord record, CancellationToken token);
}

/// <summary>What ReplayServiceAssetCode keeps of each PreviewServiceAsset response.</summary>
internal sealed class ReplayServiceAssetCodeRecord
{
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.invoicing.v1.InvoiceBatchCalculationService.</summary>
internal sealed class ReplayServiceAssetCodeInvoiceBatchCalculationWriter : IReplayServiceAssetCodeDestination
{
    private readonly ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient _client;

    public ReplayServiceAssetCodeInvoiceBatchCalculationWriter(ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient client) => _client = client;

    public async Task PushAsync(ReplayServiceAssetCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiInvoicing.SimulateInvoiceBatchRequest { Reason = record.Message }, token, _client.SimulateInvoiceBatchAsync);
    }
}

/// <summary>Sends records to acme.ledger.v1.LedgerAccountService.</summary>
internal sealed class ReplayServiceAssetCodeLedgerAccountRelay(ApiLedger.LedgerAccountService.LedgerAccountServiceClient client) : IReplayServiceAssetCodeDestination
{
    public async Task PushAsync(ReplayServiceAssetCodeRecord record, CancellationToken token)
    {
        await client.CreateLedgerAccountAsync(new ApiLedger.CreateLedgerAccountRequest { LedgerAccount = new ApiLedger.LedgerAccount { CurrencyCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.workplaces.v1.BuildingCalculationService.</summary>
internal sealed class ReplayServiceAssetCodeBuildingCalculationWriter : IReplayServiceAssetCodeDestination
{
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _client;

    public ReplayServiceAssetCodeBuildingCalculationWriter(ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient client) => _client = client;

    public async Task PushAsync(ReplayServiceAssetCodeRecord record, CancellationToken token)
    {
        await _client.GetBuildingRatesAsync(new ApiWorkplaces.GetBuildingRatesRequest { BuildingId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplayServiceAssetCodeLogOnly : IReplayServiceAssetCodeDestination
{
    public Task PushAsync(ReplayServiceAssetCodeRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Code}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.itsm.v1.ServiceAssetValidationService/PreviewServiceAsset and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayServiceAssetCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient(channel);
        var response = await source.PreviewServiceAssetAsync(new ApiItsm.PreviewServiceAssetRequest() { Reason = "current-replay-service-asset-code" }, cancellationToken: token);
        var record = new ReplayServiceAssetCodeRecord
        {
            Code = response.ServiceAsset.Code,
            Message = Text.Normalize(response.Outcome.Message),
        };
        await DeliverReplayServiceAssetCodeAsync("building-calculation", record, channel, token);
    }

    private static async Task DeliverReplayServiceAssetCodeAsync(string kind, ReplayServiceAssetCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        IReplayServiceAssetCodeDestination sink = new ReplayServiceAssetCodeLogOnly();
        switch (kind)
        {
            case "invoice-batch-calculation":
                sink = new ReplayServiceAssetCodeInvoiceBatchCalculationWriter(new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(channel));
                break;
            case "ledger-account":
                sink = new ReplayServiceAssetCodeLedgerAccountRelay(new ApiLedger.LedgerAccountService.LedgerAccountServiceClient(channel));
                break;
            case "building-calculation":
                sink = new ReplayServiceAssetCodeBuildingCalculationWriter(new ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient(channel));
                break;
        }
        await sink.PushAsync(record, token);
    }
}
