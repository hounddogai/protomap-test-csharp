using Grpc.Net.Client;
using ApiAppointments = Acme.Appointments.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where ExportGetAppointmentsStatus hands each record.</summary>
internal interface IExportGetAppointmentsStatusDestination
{
    Task WriteAsync(ExportGetAppointmentsStatusRecord record, CancellationToken token);
}

/// <summary>What ExportGetAppointmentsStatus keeps of each BatchGetAppointments response.</summary>
internal sealed class ExportGetAppointmentsStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Passes records to acme.helpdesk.v1.EscalationValidationService.</summary>
internal sealed class ExportGetAppointmentsStatusEscalationValidationRelay : IExportGetAppointmentsStatusDestination
{
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _client;

    public ExportGetAppointmentsStatusEscalationValidationRelay(ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient client) => _client = client;

    public async Task WriteAsync(ExportGetAppointmentsStatusRecord record, CancellationToken token)
    {
        await _client.NormalizeEscalationAsync(new ApiHelpdesk.NormalizeEscalationRequest { Escalation = new ApiHelpdesk.Escalation { Subject = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.corporateevents.v1.VenueSyncService.</summary>
internal sealed class ExportGetAppointmentsStatusVenueSyncSink : IExportGetAppointmentsStatusDestination
{
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _client;

    public ExportGetAppointmentsStatusVenueSyncSink(ApiCorporateevents.VenueSyncService.VenueSyncServiceClient client) => _client = client;

    public async Task WriteAsync(ExportGetAppointmentsStatusRecord record, CancellationToken token)
    {
        await _client.PullVenuesAsync(new ApiCorporateevents.PullVenuesRequest { VenueId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.helpdesk.v1.SlaPolicyValidationService.</summary>
internal sealed class ExportGetAppointmentsStatusSlaPolicyValidationWriter(ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient client) : IExportGetAppointmentsStatusDestination
{
    public async Task WriteAsync(ExportGetAppointmentsStatusRecord record, CancellationToken token)
    {
        await client.PreviewSlaPolicyAsync(new ApiHelpdesk.PreviewSlaPolicyRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.RosterApprovalService.</summary>
internal sealed class ExportGetAppointmentsStatusRosterApprovalSink : IExportGetAppointmentsStatusDestination
{
    private readonly ApiScheduling.RosterApprovalService.RosterApprovalServiceClient _client;

    public ExportGetAppointmentsStatusRosterApprovalSink(ApiScheduling.RosterApprovalService.RosterApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(ExportGetAppointmentsStatusRecord record, CancellationToken token)
    {
        await _client.RecallRosterAsync(new ApiScheduling.RecallRosterRequest { Roster = new ApiScheduling.Roster { Number = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.appointments.v1.AppointmentService/BatchGetAppointments and hands what it keeps to the sink of its kind.</summary>
internal static class ExportGetAppointmentsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAppointments.AppointmentService.AppointmentServiceClient(channel);
        var reply = await source.BatchGetAppointmentsAsync(new ApiAppointments.BatchGetAppointmentsRequest(), cancellationToken: token);
        var record = new ExportGetAppointmentsStatusRecord
        {
            Status = reply.Result.Status,
            NextPageToken = reply.PageInfo.NextPageToken,
        };
        var sinks = new Dictionary<string, IExportGetAppointmentsStatusDestination>
        {
            ["escalation-validation"] = new ExportGetAppointmentsStatusEscalationValidationRelay(new ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient(channel)),
            ["venue-sync"] = new ExportGetAppointmentsStatusVenueSyncSink(new ApiCorporateevents.VenueSyncService.VenueSyncServiceClient(channel)),
            ["sla-policy-validation"] = new ExportGetAppointmentsStatusSlaPolicyValidationWriter(new ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient(channel)),
            ["roster-approval"] = new ExportGetAppointmentsStatusRosterApprovalSink(new ApiScheduling.RosterApprovalService.RosterApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "escalation-validation")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
