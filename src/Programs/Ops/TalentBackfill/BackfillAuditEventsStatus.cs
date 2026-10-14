using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where BackfillAuditEventsStatus hands each record.</summary>
internal interface IBackfillAuditEventsStatusWriter
{
    Task WriteAsync(BackfillAuditEventsStatusRecord record, CancellationToken token);
}

/// <summary>What BackfillAuditEventsStatus keeps of each BrowseAuditEvents response.</summary>
internal sealed class BackfillAuditEventsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Writes records to acme.employeeportal.v1.SelfServiceRequestNotificationService.</summary>
internal sealed class BackfillAuditEventsStatusSelfServiceRequestNotificationTarget(ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient client) : IBackfillAuditEventsStatusWriter
{
    public async Task WriteAsync(BackfillAuditEventsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployeeportal.NotifySelfServiceRequestWatchersRequest { Details = new ApiEmployeeportal.NotifySelfServiceRequestWatchersRequest.Types.Details { Channel = record.NextPageToken }, SelfServiceRequestId = record.Message, Reason = record.CorrelationId }, token, client.NotifySelfServiceRequestWatchersAsync);
    }
}

/// <summary>Passes records to acme.helpdesk.v1.TicketApprovalService.</summary>
internal sealed class BackfillAuditEventsStatusTicketApprovalWriter : IBackfillAuditEventsStatusWriter
{
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _client;

    public BackfillAuditEventsStatusTicketApprovalWriter(ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(BackfillAuditEventsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.ListPendingTicketsRequest { Page = new ApiCommon.PageRequest { PageToken = record.NextPageToken }, Filter = new ApiHelpdesk.ListPendingTicketsRequest.Types.Filter { OwnerId = record.Status, Query = record.CorrelationId } }, token, _client.ListPendingTicketsAsync);
    }
}

/// <summary>Reads acme.audit.v1.AuditEventQueryService/BrowseAuditEvents and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillAuditEventsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient(channel);
        var found = await source.BrowseAuditEventsAsync(new ApiAudit.BrowseAuditEventsRequest() { OrderBy = "primary-backfill-audit-events-status" }, cancellationToken: token);
        var record = new BackfillAuditEventsStatusRecord
        {
            Status = found.Result.Status,
            CorrelationId = found.Result.CorrelationId,
            Message = Text.Normalize(found.Result.Message),
            NextPageToken = found.PageInfo.NextPageToken,
        };
        var sinks = new Dictionary<string, IBackfillAuditEventsStatusWriter>
        {
            ["self-request-notification"] = new BackfillAuditEventsStatusSelfServiceRequestNotificationTarget(new ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient(channel)),
            ["ticket-approval"] = new BackfillAuditEventsStatusTicketApprovalWriter(new ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "self-request-notification")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
