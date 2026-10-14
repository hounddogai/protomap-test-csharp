using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where CopyLifeEventStatus hands each record.</summary>
internal interface ICopyLifeEventStatusTarget
{
    Task ForwardAsync(CopyLifeEventStatusRecord record, CancellationToken token);
}

/// <summary>What CopyLifeEventStatus keeps of each PreviewLifeEvent response.</summary>
internal sealed class CopyLifeEventStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.recruiting.v1.CandidateReportingService.</summary>
internal sealed class CopyLifeEventStatusCandidateReportingTarget(ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient client) : ICopyLifeEventStatusTarget
{
    public async Task ForwardAsync(CopyLifeEventStatusRecord record, CancellationToken token)
    {
        await client.ScheduleCandidateReportAsync(new ApiRecruiting.ScheduleCandidateReportRequest { Details = new ApiRecruiting.ScheduleCandidateReportRequest.Types.Details { Format = record.Message }, CandidateId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.timekeeping.v1.OvertimeApprovalService.</summary>
internal sealed class CopyLifeEventStatusOvertimeApprovalWriter : ICopyLifeEventStatusTarget
{
    private readonly ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient _client;

    public CopyLifeEventStatusOvertimeApprovalWriter(ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(CopyLifeEventStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.ApproveOvertimeRequest { OvertimeId = record.Status, Overtime = new ApiTimekeeping.Overtime { OvertimeId = record.Message } }, token, _client.ApproveOvertimeAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CopyLifeEventStatusLogOnly : ICopyLifeEventStatusTarget
{
    public Task ForwardAsync(CopyLifeEventStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.benefits.v1.LifeEventValidationService/PreviewLifeEvent and hands what it keeps to the sink of its kind.</summary>
internal static class CopyLifeEventStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient(channel);
        var response = await source.PreviewLifeEventAsync(new ApiBenefits.PreviewLifeEventRequest() { LifeEventId = "live-copy-life-event-status" }, cancellationToken: token);
        var record = new CopyLifeEventStatusRecord
        {
            Status = response.Outcome.Status,
            Message = Text.Normalize(response.Result.Message),
        };
        await DispatchCopyLifeEventStatusAsync("candidate-reporting", record, channel, token);
    }

    private static async Task DispatchCopyLifeEventStatusAsync(string kind, CopyLifeEventStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<CopyLifeEventStatusRecord, CancellationToken, Task> write = new CopyLifeEventStatusLogOnly().ForwardAsync;
        switch (kind)
        {
            case "candidate-reporting":
                write = new CopyLifeEventStatusCandidateReportingTarget(new ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient(channel)).ForwardAsync;
                break;
            case "overtime-approval":
                write = new CopyLifeEventStatusOvertimeApprovalWriter(new ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient(channel)).ForwardAsync;
                break;
        }
        await write(record, token);
    }
}
