using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where PushEmploymentContractStatus hands each record.</summary>
internal interface IPushEmploymentContractStatusSink
{
    Task ForwardAsync(PushEmploymentContractStatusRecord record, CancellationToken token);
}

/// <summary>What PushEmploymentContractStatus keeps of each ScoreEmploymentContract response.</summary>
internal sealed class PushEmploymentContractStatusRecord
{
    public string Status { get; init; } = "";
    public string EmploymentContractId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.loyalty.v1.LoyaltyMemberQueryService.</summary>
internal sealed class PushEmploymentContractStatusLoyaltyMemberQueryWriter(ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient client) : IPushEmploymentContractStatusSink
{
    public async Task ForwardAsync(PushEmploymentContractStatusRecord record, CancellationToken token)
    {
        await client.PeekLoyaltyMemberAsync(new ApiLoyalty.PeekLoyaltyMemberRequest { LoyaltyMemberId = record.EmploymentContractId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.warehousing.v1.WarehouseQueryService.</summary>
internal sealed class PushEmploymentContractStatusWarehouseQueryTarget : IPushEmploymentContractStatusSink
{
    private readonly ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient _client;

    public PushEmploymentContractStatusWarehouseQueryTarget(ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(PushEmploymentContractStatusRecord record, CancellationToken token)
    {
        await _client.CountWarehousesAsync(new ApiWarehousing.CountWarehousesRequest { Filter = new ApiWarehousing.CountWarehousesRequest.Types.Filter { OwnerId = record.Status, Region = record.EmploymentContractId, Query = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.licensing.v1.LicenseAssignmentReportingService.</summary>
internal sealed class PushEmploymentContractStatusLicenseAssignmentReportingRelay(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : IPushEmploymentContractStatusSink
{
    public async Task ForwardAsync(PushEmploymentContractStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.ShareLicenseAssignmentReportRequest { Reason = record.EmploymentContractId, LicenseAssignmentId = record.CurrencyCode }, token, client.ShareLicenseAssignmentReportAsync);
    }
}

/// <summary>Reads acme.employees.v1.EmploymentContractValidationService/ScoreEmploymentContract and hands what it keeps to the sink of its kind.</summary>
internal static class PushEmploymentContractStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient(channel);
        var response = await source.ScoreEmploymentContractAsync(new ApiEmployees.ScoreEmploymentContractRequest() { EmploymentContractId = "primary-push-employment-contract-status" }, cancellationToken: token);
        var record = new PushEmploymentContractStatusRecord
        {
            Status = response.Result.Status,
            EmploymentContractId = Text.Normalize(response.EmploymentContract.EmploymentContractId),
            CurrencyCode = response.Breakdown.CurrencyCode,
            Message = response.Result.Message,
        };
        var sinks = new Dictionary<string, IPushEmploymentContractStatusSink>
        {
            ["loyalty-member-query"] = new PushEmploymentContractStatusLoyaltyMemberQueryWriter(new ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient(channel)),
            ["warehouse-query"] = new PushEmploymentContractStatusWarehouseQueryTarget(new ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient(channel)),
            ["license-assignment-reporting"] = new PushEmploymentContractStatusLicenseAssignmentReportingRelay(new ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient(channel)),
        };
        await RoutePushEmploymentContractStatusAsync("loyalty-member-query", record, sinks, token);
    }

    private static async Task RoutePushEmploymentContractStatusAsync(string kind, PushEmploymentContractStatusRecord record, IReadOnlyDictionary<string, IPushEmploymentContractStatusSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
