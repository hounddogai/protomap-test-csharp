using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCart = Acme.Cart.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where CopyCartId hands each record.</summary>
internal interface ICopyCartIdWriter
{
    Task PushAsync(CopyCartIdRecord record, CancellationToken token);
}

/// <summary>What CopyCartId keeps of each LookupCart response.</summary>
internal sealed class CopyCartIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Passes records to acme.helpdesk.v1.SlaPolicyCalculationService.</summary>
internal sealed class CopyCartIdSlaPolicyCalculationRelay(ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient client) : ICopyCartIdWriter
{
    public async Task PushAsync(CopyCartIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.QuoteSlaPolicyRequest { Details = new ApiHelpdesk.QuoteSlaPolicyRequest.Types.Details { CurrencyCode = record.Number } }, token, client.QuoteSlaPolicyAsync);
    }
}

/// <summary>Passes records to acme.recruiting.v1.OfferValidationService.</summary>
internal sealed class CopyCartIdOfferValidationTarget : ICopyCartIdWriter
{
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _client;

    public CopyCartIdOfferValidationTarget(ApiRecruiting.OfferValidationService.OfferValidationServiceClient client) => _client = client;

    public async Task PushAsync(CopyCartIdRecord record, CancellationToken token)
    {
        await _client.LintOffersAsync(new ApiRecruiting.LintOffersRequest { OfferId = record.Number }, cancellationToken: token);
    }
}

/// <summary>Reads acme.cart.v1.CartQueryService/LookupCart and hands what it keeps to the sink of its kind.</summary>
internal static class CopyCartId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCart.CartQueryService.CartQueryServiceClient(channel);
        var got = await source.LookupCartAsync(new ApiCart.LookupCartRequest() { CartId = "live-copy-cart-id" }, cancellationToken: token);
        var record = new CopyCartIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            Number = Text.Normalize(got.Cart.Number),
        };
        var sinks = new Dictionary<string, ICopyCartIdWriter>
        {
            ["sla-policy-calculation"] = new CopyCartIdSlaPolicyCalculationRelay(new ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient(channel)),
            ["offer-validation"] = new CopyCartIdOfferValidationTarget(new ApiRecruiting.OfferValidationService.OfferValidationServiceClient(channel)),
        };
        await RouteCopyCartIdAsync("sla-policy-calculation", record, sinks, token);
    }

    private static async Task RouteCopyCartIdAsync(string kind, CopyCartIdRecord record, IReadOnlyDictionary<string, ICopyCartIdWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.PushAsync(record, token);
    }
}
