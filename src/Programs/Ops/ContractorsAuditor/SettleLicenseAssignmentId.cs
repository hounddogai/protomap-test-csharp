using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where SettleLicenseAssignmentId hands each record.</summary>
internal interface ISettleLicenseAssignmentIdSink
{
    Task ApplyAsync(SettleLicenseAssignmentIdRecord record, CancellationToken token);
}

/// <summary>What SettleLicenseAssignmentId keeps of each TerminateLicenseAssignment response.</summary>
internal sealed class SettleLicenseAssignmentIdRecord
{
    public string LicenseAssignmentId { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.corporateevents.v1.CorporateEventCalculationService.</summary>
internal sealed class SettleLicenseAssignmentIdCorporateEventCalculationTarget(ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient client) : ISettleLicenseAssignmentIdSink
{
    public async Task ApplyAsync(SettleLicenseAssignmentIdRecord record, CancellationToken token)
    {
        await client.GetCorporateEventRatesAsync(new ApiCorporateevents.GetCorporateEventRatesRequest { CorporateEventId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.sellers.v1.SellerQueryService.</summary>
internal sealed class SettleLicenseAssignmentIdSellerQuerySink(ApiSellers.SellerQueryService.SellerQueryServiceClient client) : ISettleLicenseAssignmentIdSink
{
    public async Task ApplyAsync(SettleLicenseAssignmentIdRecord record, CancellationToken token)
    {
        await client.LookupSellerAsync(new ApiSellers.LookupSellerRequest { SellerId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.licensing.v1.LicenseAssignmentLifecycleService/TerminateLicenseAssignment and hands what it keeps to the sink of its kind.</summary>
internal static class SettleLicenseAssignmentId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient(channel);
        var got = await source.TerminateLicenseAssignmentAsync(new ApiLicensing.TerminateLicenseAssignmentRequest() { Reason = "live-settle-license-assignment-id" }, cancellationToken: token);
        var record = new SettleLicenseAssignmentIdRecord
        {
            LicenseAssignmentId = Text.Normalize(got.LicenseAssignment.LicenseAssignmentId),
            Status = got.Outcome.Status,
            CorrelationId = got.Result.CorrelationId,
            Message = got.Outcome.Message,
        };
        var sinks = new Dictionary<string, ISettleLicenseAssignmentIdSink>
        {
            ["corporate-event-calculation"] = new SettleLicenseAssignmentIdCorporateEventCalculationTarget(new ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient(channel)),
            ["seller-query"] = new SettleLicenseAssignmentIdSellerQuerySink(new ApiSellers.SellerQueryService.SellerQueryServiceClient(channel)),
        };
        await RouteSettleLicenseAssignmentIdAsync("seller-query", record, sinks, token);
    }

    private static async Task RouteSettleLicenseAssignmentIdAsync(string kind, SettleLicenseAssignmentIdRecord record, IReadOnlyDictionary<string, ISettleLicenseAssignmentIdSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
