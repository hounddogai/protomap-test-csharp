using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where ReplayReviewWorkflowId hands each record.</summary>
internal interface IReplayReviewWorkflowIdSink
{
    Task DeliverAsync(ReplayReviewWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What ReplayReviewWorkflowId keeps of each RetryReviewWorkflow response.</summary>
internal sealed class ReplayReviewWorkflowIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Sends records to acme.documents.v1.DocumentExportService.</summary>
internal sealed class ReplayReviewWorkflowIdDocumentExportTarget(ApiDocuments.DocumentExportService.DocumentExportServiceClient client) : IReplayReviewWorkflowIdSink
{
    public async Task DeliverAsync(ReplayReviewWorkflowIdRecord record, CancellationToken token)
    {
        await client.ScheduleDocumentExportAsync(new ApiDocuments.ScheduleDocumentExportRequest { DocumentId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.TripApprovalService.</summary>
internal sealed class ReplayReviewWorkflowIdTripApprovalRelay(ApiTravel.TripApprovalService.TripApprovalServiceClient client) : IReplayReviewWorkflowIdSink
{
    public async Task DeliverAsync(ReplayReviewWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.ListPendingTripsRequest { OrderBy = record.Status, Filter = new ApiTravel.ListPendingTripsRequest.Types.Filter { Region = record.Number, OwnerId = record.CorrelationId } }, token, client.ListPendingTripsAsync);
    }
}

/// <summary>Reads acme.performance.v1.ReviewWorkflowService/RetryReviewWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayReviewWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient(channel);
        var found = await source.RetryReviewWorkflowAsync(new ApiPerformance.RetryReviewWorkflowRequest() { Reason = "current-replay-review-workflow-id" }, cancellationToken: token);
        var record = new ReplayReviewWorkflowIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Status = found.Result.Status,
            Number = found.Review.Number,
        };
        var sinks = new Dictionary<string, IReplayReviewWorkflowIdSink>
        {
            ["document-export"] = new ReplayReviewWorkflowIdDocumentExportTarget(new ApiDocuments.DocumentExportService.DocumentExportServiceClient(channel)),
            ["trip-approval"] = new ReplayReviewWorkflowIdTripApprovalRelay(new ApiTravel.TripApprovalService.TripApprovalServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("REPLAY_REVIEW_WORKFLOW_ID_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].DeliverAsync(record, token);
    }
}
