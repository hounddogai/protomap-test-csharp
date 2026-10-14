using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where BackfillTimesheetWorkflowStatus hands each record.</summary>
internal interface IBackfillTimesheetWorkflowStatusSink
{
    Task ApplyAsync(BackfillTimesheetWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What BackfillTimesheetWorkflowStatus keeps of each CancelTimesheetWorkflow response.</summary>
internal sealed class BackfillTimesheetWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string TimesheetId { get; init; } = "";
}

/// <summary>Sends records to acme.recruiting.v1.InterviewWorkflowService.</summary>
internal sealed class BackfillTimesheetWorkflowStatusInterviewWorkflowWriter(ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient client) : IBackfillTimesheetWorkflowStatusSink
{
    public async Task ApplyAsync(BackfillTimesheetWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.StartInterviewWorkflowRequest { Details = new ApiRecruiting.StartInterviewWorkflowRequest.Types.Details { InitiatedBy = record.Message } }, token, client.StartInterviewWorkflowAsync);
    }
}

/// <summary>Hands records to acme.broadcasts.v1.BroadcastService.</summary>
internal sealed class BackfillTimesheetWorkflowStatusBroadcastSink(ApiBroadcasts.BroadcastService.BroadcastServiceClient client) : IBackfillTimesheetWorkflowStatusSink
{
    public async Task ApplyAsync(BackfillTimesheetWorkflowStatusRecord record, CancellationToken token)
    {
        await client.CloneBroadcastAsync(new ApiBroadcasts.CloneBroadcastRequest { Broadcast = new ApiBroadcasts.Broadcast { Title = record.Message, BroadcastId = record.TimesheetId }, RequestId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.timekeeping.v1.TimesheetWorkflowService/CancelTimesheetWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillTimesheetWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient(channel);
        var response = await source.CancelTimesheetWorkflowAsync(new ApiTimekeeping.CancelTimesheetWorkflowRequest() { Reason = "nightly-backfill-timesheet-workflow-status" }, cancellationToken: token);
        var record = new BackfillTimesheetWorkflowStatusRecord
        {
            Status = response.Outcome.Status,
            Message = response.Result.Message,
            TimesheetId = response.Timesheet.TimesheetId,
        };
        await ActOnBackfillTimesheetWorkflowStatusAsync("broadcast", record, channel, token);
    }

    private static async Task ActOnBackfillTimesheetWorkflowStatusAsync(string kind, BackfillTimesheetWorkflowStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["interview-workflow"] = () => new BackfillTimesheetWorkflowStatusInterviewWorkflowWriter(new ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient(channel)).ApplyAsync(record, token),
            ["broadcast"] = () => new BackfillTimesheetWorkflowStatusBroadcastSink(new ApiBroadcasts.BroadcastService.BroadcastServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
