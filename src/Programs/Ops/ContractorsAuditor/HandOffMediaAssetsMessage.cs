using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where HandOffMediaAssetsMessage hands each record.</summary>
internal interface IHandOffMediaAssetsMessageWriter
{
    Task ApplyAsync(HandOffMediaAssetsMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffMediaAssetsMessage keeps of each SearchMediaAssets response.</summary>
internal sealed class HandOffMediaAssetsMessageRecord
{
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Writes records to acme.learning.v1.CourseApprovalService.</summary>
internal sealed class HandOffMediaAssetsMessageCourseApprovalSink : IHandOffMediaAssetsMessageWriter
{
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _client;

    public HandOffMediaAssetsMessageCourseApprovalSink(ApiLearning.CourseApprovalService.CourseApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(HandOffMediaAssetsMessageRecord record, CancellationToken token)
    {
        await _client.RecallCourseAsync(new ApiLearning.RecallCourseRequest { Details = new ApiLearning.RecallCourseRequest.Types.Details { ApproverEmail = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.EquipmentWorkflowService.</summary>
internal sealed class HandOffMediaAssetsMessageEquipmentWorkflowWriter(ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient client) : IHandOffMediaAssetsMessageWriter
{
    public async Task ApplyAsync(HandOffMediaAssetsMessageRecord record, CancellationToken token)
    {
        await client.SignalEquipmentWorkflowAsync(new ApiOnboarding.SignalEquipmentWorkflowRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.employees.v1.EmployeeWorkflowService.</summary>
internal sealed class HandOffMediaAssetsMessageEmployeeWorkflowWriter : IHandOffMediaAssetsMessageWriter
{
    private readonly ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient _client;

    public HandOffMediaAssetsMessageEmployeeWorkflowWriter(ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient client) => _client = client;

    public async Task ApplyAsync(HandOffMediaAssetsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.RetryEmployeeWorkflowRequest { Employee = new ApiEmployees.Employee { EmployeeId = record.Message }, EmployeeId = record.NextPageToken }, token, _client.RetryEmployeeWorkflowAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffMediaAssetsMessageLogOnly : IHandOffMediaAssetsMessageWriter
{
    public Task ApplyAsync(HandOffMediaAssetsMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.media.v1.MediaAssetService/SearchMediaAssets and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffMediaAssetsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMedia.MediaAssetService.MediaAssetServiceClient(channel);
        var found = await source.SearchMediaAssetsAsync(new ApiMedia.SearchMediaAssetsRequest() { OrderBy = "live-hand-off-media-assets-message" }, cancellationToken: token);
        var record = new HandOffMediaAssetsMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            NextPageToken = found.PageInfo.NextPageToken,
        };
        await DeliverHandOffMediaAssetsMessageAsync("employee-workflow", record, channel, token);
    }

    private static async Task DeliverHandOffMediaAssetsMessageAsync(string kind, HandOffMediaAssetsMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        IHandOffMediaAssetsMessageWriter sink = kind switch
        {
            "course-approval" => new HandOffMediaAssetsMessageCourseApprovalSink(new ApiLearning.CourseApprovalService.CourseApprovalServiceClient(channel)),
            "equipment-workflow" => new HandOffMediaAssetsMessageEquipmentWorkflowWriter(new ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient(channel)),
            "employee-workflow" => new HandOffMediaAssetsMessageEmployeeWorkflowWriter(new ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient(channel)),
            _ => new HandOffMediaAssetsMessageLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
