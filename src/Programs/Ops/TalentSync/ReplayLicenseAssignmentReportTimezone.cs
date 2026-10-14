using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>What ReplayLicenseAssignmentReportTimezone keeps of each GenerateLicenseAssignmentReport response.</summary>
internal sealed class ReplayLicenseAssignmentReportTimezoneRecord
{
    public string Timezone { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.ledger.v1.LedgerAccountValidationService.</summary>
internal sealed class ReplayLicenseAssignmentReportTimezoneLedgerAccountValidationWriter
{
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _client;

    public ReplayLicenseAssignmentReportTimezoneLedgerAccountValidationWriter(ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(ReplayLicenseAssignmentReportTimezoneRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLedger.LintLedgerAccountsRequest { Details = new ApiLedger.LintLedgerAccountsRequest.Types.Details { Locale = record.CorrelationId }, LedgerAccount = new ApiLedger.LedgerAccount { CurrencyCode = record.Message }, LedgerAccountId = record.Timezone }, token, _client.LintLedgerAccountsAsync);
    }
}

/// <summary>Reads acme.licensing.v1.LicenseAssignmentReportingService/GenerateLicenseAssignmentReport and hands what it keeps to acme.ledger.v1.LedgerAccountValidationService.</summary>
internal static class ReplayLicenseAssignmentReportTimezone
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient(channel);
        var found = await source.GenerateLicenseAssignmentReportAsync(new ApiLicensing.GenerateLicenseAssignmentReportRequest() { LicenseAssignmentId = "current-replay-license-assignment-report-timezone" }, cancellationToken: token);
        var record = new ReplayLicenseAssignmentReportTimezoneRecord
        {
            Timezone = Text.Normalize(found.Period.Timezone),
            Message = found.Result.Message,
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
        };
        await new ReplayLicenseAssignmentReportTimezoneLedgerAccountValidationWriter(new ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient(channel)).DeliverAsync(record, token);
    }
}
