using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiContent = Acme.Content.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where ReplayCertificationWorkflowId hands each record.</summary>
internal interface IReplayCertificationWorkflowIdDestination
{
    Task ForwardAsync(ReplayCertificationWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What ReplayCertificationWorkflowId keeps of each StartCertificationWorkflow response.</summary>
internal sealed class ReplayCertificationWorkflowIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Notes { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
}

/// <summary>Passes records to acme.performance.v1.GoalApprovalService.</summary>
internal sealed class ReplayCertificationWorkflowIdGoalApprovalTarget : IReplayCertificationWorkflowIdDestination
{
    private readonly ApiPerformance.GoalApprovalService.GoalApprovalServiceClient _client;

    public ReplayCertificationWorkflowIdGoalApprovalTarget(ApiPerformance.GoalApprovalService.GoalApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayCertificationWorkflowIdRecord record, CancellationToken token)
    {
        await _client.SubmitGoalAsync(new ApiPerformance.SubmitGoalRequest { Goal = new ApiPerformance.Goal { GoalId = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.content.v1.ArticleService.</summary>
internal sealed class ReplayCertificationWorkflowIdArticleWriter : IReplayCertificationWorkflowIdDestination
{
    private readonly ApiContent.ArticleService.ArticleServiceClient _client;

    public ReplayCertificationWorkflowIdArticleWriter(ApiContent.ArticleService.ArticleServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayCertificationWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiContent.DeleteArticleRequest { ArticleId = record.OutcomeStatus, Reason = record.CorrelationId, Etag = record.Status }, token, _client.DeleteArticleAsync);
    }
}

/// <summary>Reads acme.learning.v1.CertificationWorkflowService/StartCertificationWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayCertificationWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient(channel);
        var reply = await source.StartCertificationWorkflowAsync(new ApiLearning.StartCertificationWorkflowRequest() { Reason = "live-replay-certification-workflow-id" }, cancellationToken: token);
        var record = new ReplayCertificationWorkflowIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Status = Text.Normalize(reply.Result.Status),
            Notes = reply.Certification.Notes,
            OutcomeStatus = reply.Outcome.Status,
        };
        var sinks = new Dictionary<string, IReplayCertificationWorkflowIdDestination>
        {
            ["goal-approval"] = new ReplayCertificationWorkflowIdGoalApprovalTarget(new ApiPerformance.GoalApprovalService.GoalApprovalServiceClient(channel)),
            ["article"] = new ReplayCertificationWorkflowIdArticleWriter(new ApiContent.ArticleService.ArticleServiceClient(channel)),
        };
        await RouteReplayCertificationWorkflowIdAsync("article", record, sinks, token);
    }

    private static async Task RouteReplayCertificationWorkflowIdAsync(string kind, ReplayCertificationWorkflowIdRecord record, IReadOnlyDictionary<string, IReplayCertificationWorkflowIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
