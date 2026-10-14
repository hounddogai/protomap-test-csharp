using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where PushDocumentTemplateWorkflowStatus hands each record.</summary>
internal interface IPushDocumentTemplateWorkflowStatusDestination
{
    Task WriteAsync(PushDocumentTemplateWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What PushDocumentTemplateWorkflowStatus keeps of each ResumeDocumentTemplateWorkflow response.</summary>
internal sealed class PushDocumentTemplateWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Writes records to acme.performance.v1.ReviewApprovalService.</summary>
internal sealed class PushDocumentTemplateWorkflowStatusReviewApprovalRelay : IPushDocumentTemplateWorkflowStatusDestination
{
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _client;

    public PushDocumentTemplateWorkflowStatusReviewApprovalRelay(ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(PushDocumentTemplateWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.CommentReviewApprovalRequest { Details = new ApiPerformance.CommentReviewApprovalRequest.Types.Details { ApproverEmail = record.Notes }, Reason = record.CorrelationId }, token, _client.CommentReviewApprovalAsync);
    }
}

/// <summary>Writes records to acme.recruiting.v1.CandidateReportingService.</summary>
internal sealed class PushDocumentTemplateWorkflowStatusCandidateReportingTarget : IPushDocumentTemplateWorkflowStatusDestination
{
    private readonly ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient _client;

    public PushDocumentTemplateWorkflowStatusCandidateReportingTarget(ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient client) => _client = client;

    public async Task WriteAsync(PushDocumentTemplateWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.ListCandidateReportsRequest { Filter = new ApiRecruiting.ListCandidateReportsRequest.Types.Filter { Query = record.Notes } }, token, _client.ListCandidateReportsAsync);
    }
}

/// <summary>Reads acme.documents.v1.DocumentTemplateWorkflowService/ResumeDocumentTemplateWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class PushDocumentTemplateWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient(channel);
        var reply = await source.ResumeDocumentTemplateWorkflowAsync(new ApiDocuments.ResumeDocumentTemplateWorkflowRequest() { DocumentTemplateId = "default-push-document-template-workflow-status" }, cancellationToken: token);
        var record = new PushDocumentTemplateWorkflowStatusRecord
        {
            Status = reply.Outcome.Status,
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
            Notes = reply.DocumentTemplate.Notes,
        };
        var sinks = new Dictionary<string, IPushDocumentTemplateWorkflowStatusDestination>
        {
            ["review-approval"] = new PushDocumentTemplateWorkflowStatusReviewApprovalRelay(new ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient(channel)),
            ["candidate-reporting"] = new PushDocumentTemplateWorkflowStatusCandidateReportingTarget(new ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "review-approval")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
