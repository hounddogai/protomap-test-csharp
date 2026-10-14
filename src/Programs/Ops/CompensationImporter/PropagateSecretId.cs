using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCheckout = Acme.Checkout.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiSecrets = Acme.Secrets.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where PropagateSecretId hands each record.</summary>
internal interface IPropagateSecretIdDestination
{
    Task PushAsync(PropagateSecretIdRecord record, CancellationToken token);
}

/// <summary>What PropagateSecretId keeps of each UpdateSecret response.</summary>
internal sealed class PropagateSecretIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Hands records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class PropagateSecretIdCheckoutSessionTarget : IPropagateSecretIdDestination
{
    private readonly ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient _client;

    public PropagateSecretIdCheckoutSessionTarget(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) => _client = client;

    public async Task PushAsync(PropagateSecretIdRecord record, CancellationToken token)
    {
        await _client.SearchCheckoutSessionsAsync(new ApiCheckout.SearchCheckoutSessionsRequest { Filter = new ApiCheckout.SearchCheckoutSessionsRequest.Types.Filter { Region = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.ledger.v1.LedgerAccountService.</summary>
internal sealed class PropagateSecretIdLedgerAccountSink(ApiLedger.LedgerAccountService.LedgerAccountServiceClient client) : IPropagateSecretIdDestination
{
    public async Task PushAsync(PropagateSecretIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLedger.CreateLedgerAccountRequest { LedgerAccount = new ApiLedger.LedgerAccount { CurrencyCode = record.CorrelationId } }, token, client.CreateLedgerAccountAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateSecretIdLogOnly : IPropagateSecretIdDestination
{
    public Task PushAsync(PropagateSecretIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.secrets.v1.SecretService/UpdateSecret and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateSecretId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiSecrets.SecretService.SecretServiceClient(channel);
        var found = await source.UpdateSecretAsync(new ApiSecrets.UpdateSecretRequest(), cancellationToken: token);
        var record = new PropagateSecretIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            CurrencyCode = found.Secret.CurrencyCode,
        };
        await DeliverPropagateSecretIdAsync("checkout-session", record, channel, token);
    }

    private static async Task DeliverPropagateSecretIdAsync(string kind, PropagateSecretIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagateSecretIdDestination sink = kind switch
        {
            "checkout-session" => new PropagateSecretIdCheckoutSessionTarget(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel)),
            "ledger-account" => new PropagateSecretIdLedgerAccountSink(new ApiLedger.LedgerAccountService.LedgerAccountServiceClient(channel)),
            _ => new PropagateSecretIdLogOnly(),
        };
        await sink.PushAsync(record, token);
    }
}
