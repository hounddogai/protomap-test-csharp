using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where SyncLedgerAccountsToken hands each record.</summary>
internal interface ISyncLedgerAccountsTokenSink
{
    Task WriteAsync(SyncLedgerAccountsTokenRecord record, CancellationToken token);
}

/// <summary>What SyncLedgerAccountsToken keeps of each SuggestLedgerAccounts response.</summary>
internal sealed class SyncLedgerAccountsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.itsm.v1.ProblemLifecycleService.</summary>
internal sealed class SyncLedgerAccountsTokenProblemLifecycleSink(ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient client) : ISyncLedgerAccountsTokenSink
{
    public async Task WriteAsync(SyncLedgerAccountsTokenRecord record, CancellationToken token)
    {
        await client.RenewProblemAsync(new ApiItsm.RenewProblemRequest { Details = new ApiItsm.RenewProblemRequest.Types.Details { ReasonCode = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.drivers.v1.DriverCalculationService.</summary>
internal sealed class SyncLedgerAccountsTokenDriverCalculationWriter(ApiDrivers.DriverCalculationService.DriverCalculationServiceClient client) : ISyncLedgerAccountsTokenSink
{
    public async Task WriteAsync(SyncLedgerAccountsTokenRecord record, CancellationToken token)
    {
        await client.ExplainDriverCalculationAsync(new ApiDrivers.ExplainDriverCalculationRequest { Reason = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.fuel.v1.FuelCardService.</summary>
internal sealed class SyncLedgerAccountsTokenFuelCardSink(ApiFuel.FuelCardService.FuelCardServiceClient client) : ISyncLedgerAccountsTokenSink
{
    public async Task WriteAsync(SyncLedgerAccountsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFuel.ListFuelCardsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Message } }, token, client.ListFuelCardsAsync);
    }
}

/// <summary>Reads acme.ledger.v1.LedgerAccountQueryService/SuggestLedgerAccounts and hands what it keeps to the sink of its kind.</summary>
internal static class SyncLedgerAccountsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient(channel);
        var reply = await source.SuggestLedgerAccountsAsync(new ApiLedger.SuggestLedgerAccountsRequest() { OrderBy = "primary-sync-ledger-accounts-token" }, cancellationToken: token);
        var record = new SyncLedgerAccountsTokenRecord
        {
            NextPageToken = reply.PageInfo.NextPageToken,
            CorrelationId = reply.Result.CorrelationId,
            Message = reply.Result.Message,
        };
        var sinks = new Dictionary<string, ISyncLedgerAccountsTokenSink>
        {
            ["problem-lifecycle"] = new SyncLedgerAccountsTokenProblemLifecycleSink(new ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient(channel)),
            ["driver-calculation"] = new SyncLedgerAccountsTokenDriverCalculationWriter(new ApiDrivers.DriverCalculationService.DriverCalculationServiceClient(channel)),
            ["fuel-card"] = new SyncLedgerAccountsTokenFuelCardSink(new ApiFuel.FuelCardService.FuelCardServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "problem-lifecycle")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
