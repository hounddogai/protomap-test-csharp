using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiLearning = Acme.Learning.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where SyncInterviewWorkflowId hands each record.</summary>
internal interface ISyncInterviewWorkflowIdSink
{
    Task SendAsync(SyncInterviewWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What SyncInterviewWorkflowId keeps of each AdvanceInterviewWorkflow response.</summary>
internal sealed class SyncInterviewWorkflowIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
    public string InterviewId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.employees.v1.EmployeeApprovalService.</summary>
internal sealed class SyncInterviewWorkflowIdEmployeeApprovalRelay(ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient client) : ISyncInterviewWorkflowIdSink
{
    public async Task SendAsync(SyncInterviewWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.ListPendingEmployeesRequest { Filter = new ApiEmployees.ListPendingEmployeesRequest.Types.Filter { Query = record.InterviewId } }, token, client.ListPendingEmployeesAsync);
    }
}

/// <summary>Passes records to acme.learning.v1.CourseWorkflowService.</summary>
internal sealed class SyncInterviewWorkflowIdCourseWorkflowRelay : ISyncInterviewWorkflowIdSink
{
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _client;

    public SyncInterviewWorkflowIdCourseWorkflowRelay(ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(SyncInterviewWorkflowIdRecord record, CancellationToken token)
    {
        await _client.StartCourseWorkflowAsync(new ApiLearning.StartCourseWorkflowRequest { Details = new ApiLearning.StartCourseWorkflowRequest.Types.Details { InitiatedBy = record.InterviewId }, Course = new ApiLearning.Course { Code = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingTaskApprovalService.</summary>
internal sealed class SyncInterviewWorkflowIdOnboardingTaskApprovalSink(ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient client) : ISyncInterviewWorkflowIdSink
{
    public async Task SendAsync(SyncInterviewWorkflowIdRecord record, CancellationToken token)
    {
        await client.ApproveOnboardingTaskAsync(new ApiOnboarding.ApproveOnboardingTaskRequest { Details = new ApiOnboarding.ApproveOnboardingTaskRequest.Types.Details { Comment = record.InterviewId }, Reason = record.Message, OnboardingTask = new ApiOnboarding.OnboardingTask { Subject = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncInterviewWorkflowIdLogOnly : ISyncInterviewWorkflowIdSink
{
    public Task SendAsync(SyncInterviewWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReferenceId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.recruiting.v1.InterviewWorkflowService/AdvanceInterviewWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class SyncInterviewWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient(channel);
        var found = await source.AdvanceInterviewWorkflowAsync(new ApiRecruiting.AdvanceInterviewWorkflowRequest() { InterviewId = "live-sync-interview-workflow-id" }, cancellationToken: token);
        var record = new SyncInterviewWorkflowIdRecord
        {
            ReferenceId = found.Outcome.ReferenceId,
            Message = Text.Normalize(found.Outcome.Message),
            InterviewId = Text.Normalize(found.Interview.InterviewId),
            CorrelationId = found.Result.CorrelationId,
        };
        await DeliverSyncInterviewWorkflowIdAsync("employee-approval", record, channel, token);
    }

    private static async Task DeliverSyncInterviewWorkflowIdAsync(string kind, SyncInterviewWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISyncInterviewWorkflowIdSink sink = kind switch
        {
            "employee-approval" => new SyncInterviewWorkflowIdEmployeeApprovalRelay(new ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient(channel)),
            "course-workflow" => new SyncInterviewWorkflowIdCourseWorkflowRelay(new ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient(channel)),
            "onboarding-task-approval" => new SyncInterviewWorkflowIdOnboardingTaskApprovalSink(new ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient(channel)),
            _ => new SyncInterviewWorkflowIdLogOnly(),
        };
        await sink.SendAsync(record, token);
    }
}
