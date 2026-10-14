using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where MirrorDocumentsStatus hands each record.</summary>
internal interface IMirrorDocumentsStatusTarget
{
    Task ForwardAsync(MirrorDocumentsStatusRecord record, CancellationToken token);
}

/// <summary>What MirrorDocumentsStatus keeps of each FindDocuments response.</summary>
internal sealed class MirrorDocumentsStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Writes records to acme.helpdesk.v1.TicketQueryService.</summary>
internal sealed class MirrorDocumentsStatusTicketQueryTarget : IMirrorDocumentsStatusTarget
{
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _client;

    public MirrorDocumentsStatusTicketQueryTarget(ApiHelpdesk.TicketQueryService.TicketQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(MirrorDocumentsStatusRecord record, CancellationToken token)
    {
        await _client.SummarizeTicketsAsync(new ApiHelpdesk.SummarizeTicketsRequest { TicketId = record.Status, Details = new ApiHelpdesk.SummarizeTicketsRequest.Types.Details { Format = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.timekeeping.v1.TimesheetWorkflowService.</summary>
internal sealed class MirrorDocumentsStatusTimesheetWorkflowWriter(ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient client) : IMirrorDocumentsStatusTarget
{
    public async Task ForwardAsync(MirrorDocumentsStatusRecord record, CancellationToken token)
    {
        await client.AdvanceTimesheetWorkflowAsync(new ApiTimekeeping.AdvanceTimesheetWorkflowRequest { Timesheet = new ApiTimekeeping.Timesheet { Notes = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.licensing.v1.LicenseAssignmentWorkflowService.</summary>
internal sealed class MirrorDocumentsStatusLicenseAssignmentWorkflowWriter(ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient client) : IMirrorDocumentsStatusTarget
{
    public async Task ForwardAsync(MirrorDocumentsStatusRecord record, CancellationToken token)
    {
        await client.AdvanceLicenseAssignmentWorkflowAsync(new ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest { Details = new ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest.Types.Details { InitiatedBy = record.NextPageToken }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.drivers.v1.DriverCalculationService.</summary>
internal sealed class MirrorDocumentsStatusDriverCalculationSink : IMirrorDocumentsStatusTarget
{
    private readonly ApiDrivers.DriverCalculationService.DriverCalculationServiceClient _client;

    public MirrorDocumentsStatusDriverCalculationSink(ApiDrivers.DriverCalculationService.DriverCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(MirrorDocumentsStatusRecord record, CancellationToken token)
    {
        await _client.QuoteDriverAsync(new ApiDrivers.QuoteDriverRequest { Driver = new ApiDrivers.Driver { DriverId = record.Status }, Reason = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentQueryService/FindDocuments and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorDocumentsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentQueryService.DocumentQueryServiceClient(channel);
        var found = await source.FindDocumentsAsync(new ApiDocuments.FindDocumentsRequest() { OrderBy = "current-mirror-documents-status" }, cancellationToken: token);
        var record = new MirrorDocumentsStatusRecord
        {
            Status = found.Result.Status,
            NextPageToken = Text.Normalize(found.PageInfo.NextPageToken),
        };
        await ActOnMirrorDocumentsStatusAsync("driver-calculation", record, channel, token);
    }

    private static async Task ActOnMirrorDocumentsStatusAsync(string kind, MirrorDocumentsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["ticket-query"] = () => new MirrorDocumentsStatusTicketQueryTarget(new ApiHelpdesk.TicketQueryService.TicketQueryServiceClient(channel)).ForwardAsync(record, token),
            ["timesheet-workflow"] = () => new MirrorDocumentsStatusTimesheetWorkflowWriter(new ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient(channel)).ForwardAsync(record, token),
            ["license-assignment-workflow"] = () => new MirrorDocumentsStatusLicenseAssignmentWorkflowWriter(new ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient(channel)).ForwardAsync(record, token),
            ["driver-calculation"] = () => new MirrorDocumentsStatusDriverCalculationSink(new ApiDrivers.DriverCalculationService.DriverCalculationServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
