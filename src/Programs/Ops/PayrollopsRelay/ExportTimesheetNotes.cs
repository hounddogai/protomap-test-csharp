using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiTelematics = Acme.Telematics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where ExportTimesheetNotes hands each record.</summary>
internal interface IExportTimesheetNotesWriter
{
    Task ForwardAsync(ExportTimesheetNotesRecord record, CancellationToken token);
}

/// <summary>What ExportTimesheetNotes keeps of each RecallTimesheet response.</summary>
internal sealed class ExportTimesheetNotesRecord
{
    public string Notes { get; init; } = "";
    public string TimesheetId { get; init; } = "";
}

/// <summary>Hands records to acme.fleet.v1.VehicleQueryService.</summary>
internal sealed class ExportTimesheetNotesVehicleQuerySink(ApiFleet.VehicleQueryService.VehicleQueryServiceClient client) : IExportTimesheetNotesWriter
{
    public async Task ForwardAsync(ExportTimesheetNotesRecord record, CancellationToken token)
    {
        await client.CountVehiclesAsync(new ApiFleet.CountVehiclesRequest { OrderBy = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.employeeportal.v1.PortalHomeNotificationService.</summary>
internal sealed class ExportTimesheetNotesPortalHomeNotificationTarget(ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient client) : IExportTimesheetNotesWriter
{
    public async Task ForwardAsync(ExportTimesheetNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployeeportal.GetPortalHomePreferencesRequest { PortalHomeId = record.Notes }, token, client.GetPortalHomePreferencesAsync);
    }
}

/// <summary>Writes records to acme.telematics.v1.TelemetryReadingQueryService.</summary>
internal sealed class ExportTimesheetNotesTelemetryReadingQueryTarget : IExportTimesheetNotesWriter
{
    private readonly ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient _client;

    public ExportTimesheetNotesTelemetryReadingQueryTarget(ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(ExportTimesheetNotesRecord record, CancellationToken token)
    {
        await _client.FindTelemetryReadingsAsync(new ApiTelematics.FindTelemetryReadingsRequest { Filter = new ApiTelematics.FindTelemetryReadingsRequest.Types.Filter { OwnerId = record.TimesheetId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.timekeeping.v1.TimesheetApprovalService/RecallTimesheet and hands what it keeps to the sink of its kind.</summary>
internal static class ExportTimesheetNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient(channel);
        var found = await source.RecallTimesheetAsync(new ApiTimekeeping.RecallTimesheetRequest() { Reason = "current-export-timesheet-notes" }, cancellationToken: token);
        var record = new ExportTimesheetNotesRecord
        {
            Notes = found.Timesheet.Notes,
            TimesheetId = found.Timesheet.TimesheetId,
        };
        var sinks = new Dictionary<string, IExportTimesheetNotesWriter>
        {
            ["vehicle-query"] = new ExportTimesheetNotesVehicleQuerySink(new ApiFleet.VehicleQueryService.VehicleQueryServiceClient(channel)),
            ["portal-home-notification"] = new ExportTimesheetNotesPortalHomeNotificationTarget(new ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient(channel)),
            ["telemetry-reading-query"] = new ExportTimesheetNotesTelemetryReadingQueryTarget(new ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "vehicle-query")
                continue;
            await sink.ForwardAsync(record, token);
        }
    }
}
