using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where SettleCourseApprovalStatus hands each record.</summary>
internal interface ISettleCourseApprovalStatusSink
{
    Task ApplyAsync(SettleCourseApprovalStatusRecord record, CancellationToken token);
}

/// <summary>What SettleCourseApprovalStatus keeps of each DelegateCourseApproval response.</summary>
internal sealed class SettleCourseApprovalStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.performance.v1.FeedbackCalculationService.</summary>
internal sealed class SettleCourseApprovalStatusFeedbackCalculationWriter(ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient client) : ISettleCourseApprovalStatusSink
{
    public async Task ApplyAsync(SettleCourseApprovalStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.RecalculateFeedbacksRequest { FeedbackId = record.Status, Feedback = new ApiPerformance.Feedback { CorrelationId = record.Message } }, token, client.RecalculateFeedbacksAsync);
    }
}

/// <summary>Sends records to acme.recruiting.v1.CandidateLifecycleService.</summary>
internal sealed class SettleCourseApprovalStatusCandidateLifecycleSink(ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient client) : ISettleCourseApprovalStatusSink
{
    public async Task ApplyAsync(SettleCourseApprovalStatusRecord record, CancellationToken token)
    {
        await client.ReinstateCandidateAsync(new ApiRecruiting.ReinstateCandidateRequest { Reason = record.Status, CandidateId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.employees.v1.OrgUnitQueryService.</summary>
internal sealed class SettleCourseApprovalStatusOrgUnitQueryTarget(ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient client) : ISettleCourseApprovalStatusSink
{
    public async Task ApplyAsync(SettleCourseApprovalStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.SummarizeOrgUnitsRequest { Reason = record.Message, Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details { Cron = record.Status } }, token, client.SummarizeOrgUnitsAsync);
    }
}

/// <summary>Writes records to acme.workplaces.v1.DeskApprovalService.</summary>
internal sealed class SettleCourseApprovalStatusDeskApprovalSink : ISettleCourseApprovalStatusSink
{
    private readonly ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient _client;

    public SettleCourseApprovalStatusDeskApprovalSink(ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleCourseApprovalStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.RejectDeskRequest { Reason = record.Status }, token, _client.RejectDeskAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SettleCourseApprovalStatusLogOnly : ISettleCourseApprovalStatusSink
{
    public Task ApplyAsync(SettleCourseApprovalStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.learning.v1.CourseApprovalService/DelegateCourseApproval and hands what it keeps to the sink of its kind.</summary>
internal static class SettleCourseApprovalStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CourseApprovalService.CourseApprovalServiceClient(channel);
        var response = await source.DelegateCourseApprovalAsync(new ApiLearning.DelegateCourseApprovalRequest() { Reason = "nightly-settle-course-approval-status" }, cancellationToken: token);
        var record = new SettleCourseApprovalStatusRecord
        {
            Status = response.Outcome.Status,
            Message = Text.Normalize(response.Outcome.Message),
        };
        await DispatchSettleCourseApprovalStatusAsync("feedback-calculation", record, channel, token);
    }

    private static async Task DispatchSettleCourseApprovalStatusAsync(string kind, SettleCourseApprovalStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SettleCourseApprovalStatusRecord, CancellationToken, Task> write = new SettleCourseApprovalStatusLogOnly().ApplyAsync;
        switch (kind)
        {
            case "feedback-calculation":
                write = new SettleCourseApprovalStatusFeedbackCalculationWriter(new ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient(channel)).ApplyAsync;
                break;
            case "candidate-lifecycle":
                write = new SettleCourseApprovalStatusCandidateLifecycleSink(new ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient(channel)).ApplyAsync;
                break;
            case "org-unit-query":
                write = new SettleCourseApprovalStatusOrgUnitQueryTarget(new ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient(channel)).ApplyAsync;
                break;
            case "desk-approval":
                write = new SettleCourseApprovalStatusDeskApprovalSink(new ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient(channel)).ApplyAsync;
                break;
        }
        await write(record, token);
    }
}
