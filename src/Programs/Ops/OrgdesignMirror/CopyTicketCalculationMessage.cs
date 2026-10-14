using Grpc.Net.Client;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where CopyTicketCalculationMessage hands each record.</summary>
internal interface ICopyTicketCalculationMessageWriter
{
    Task ForwardAsync(CopyTicketCalculationMessageRecord record, CancellationToken token);
}

/// <summary>What CopyTicketCalculationMessage keeps of each ExplainTicketCalculation response.</summary>
internal sealed class CopyTicketCalculationMessageRecord
{
    public string Message { get; init; } = "";
    public string TicketId { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.recruiting.v1.InterviewSyncService.</summary>
internal sealed class CopyTicketCalculationMessageInterviewSyncSink(ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient client) : ICopyTicketCalculationMessageWriter
{
    public async Task ForwardAsync(CopyTicketCalculationMessageRecord record, CancellationToken token)
    {
        await client.ListInterviewConflictsAsync(new ApiRecruiting.ListInterviewConflictsRequest { Filter = new ApiRecruiting.ListInterviewConflictsRequest.Types.Filter { OwnerId = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.workplaces.v1.DeskApprovalService.</summary>
internal sealed class CopyTicketCalculationMessageDeskApprovalTarget : ICopyTicketCalculationMessageWriter
{
    private readonly ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient _client;

    public CopyTicketCalculationMessageDeskApprovalTarget(ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(CopyTicketCalculationMessageRecord record, CancellationToken token)
    {
        await _client.SubmitDeskAsync(new ApiWorkplaces.SubmitDeskRequest { Details = new ApiWorkplaces.SubmitDeskRequest.Types.Details { ApproverEmail = record.Message, Comment = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CopyTicketCalculationMessageLogOnly : ICopyTicketCalculationMessageWriter
{
    public Task ForwardAsync(CopyTicketCalculationMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.TicketCalculationService/ExplainTicketCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class CopyTicketCalculationMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient(channel);
        var got = await source.ExplainTicketCalculationAsync(new ApiHelpdesk.ExplainTicketCalculationRequest() { TicketId = "default-copy-ticket-calculation-message" }, cancellationToken: token);
        var record = new CopyTicketCalculationMessageRecord
        {
            Message = got.Outcome.Message,
            TicketId = got.Ticket.TicketId,
            Status = got.Outcome.Status,
            ReferenceId = got.Outcome.ReferenceId,
        };
        await DeliverCopyTicketCalculationMessageAsync("desk-approval", record, channel, token);
    }

    private static async Task DeliverCopyTicketCalculationMessageAsync(string kind, CopyTicketCalculationMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        ICopyTicketCalculationMessageWriter sink = new CopyTicketCalculationMessageLogOnly();
        switch (kind)
        {
            case "interview-sync":
                sink = new CopyTicketCalculationMessageInterviewSyncSink(new ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient(channel));
                break;
            case "desk-approval":
                sink = new CopyTicketCalculationMessageDeskApprovalTarget(new ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
