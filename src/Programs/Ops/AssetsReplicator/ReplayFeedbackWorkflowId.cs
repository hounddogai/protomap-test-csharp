using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where ReplayFeedbackWorkflowId hands each record.</summary>
internal interface IReplayFeedbackWorkflowIdDestination
{
    Task WriteAsync(ReplayFeedbackWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What ReplayFeedbackWorkflowId keeps of each CancelFeedbackWorkflow response.</summary>
internal sealed class ReplayFeedbackWorkflowIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Kind { get; init; } = "";
}

/// <summary>Hands records to acme.corporateevents.v1.CorporateEventWorkflowService.</summary>
internal sealed class ReplayFeedbackWorkflowIdCorporateEventWorkflowSink : IReplayFeedbackWorkflowIdDestination
{
    private readonly ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient _client;

    public ReplayFeedbackWorkflowIdCorporateEventWorkflowSink(ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(ReplayFeedbackWorkflowIdRecord record, CancellationToken token)
    {
        await _client.AdvanceCorporateEventWorkflowAsync(new ApiCorporateevents.AdvanceCorporateEventWorkflowRequest { CorporateEvent = new ApiCorporateevents.CorporateEvent { CorporateEventId = record.CorrelationId }, CorporateEventId = record.Kind }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.RosterApprovalService.</summary>
internal sealed class ReplayFeedbackWorkflowIdRosterApprovalWriter(ApiScheduling.RosterApprovalService.RosterApprovalServiceClient client) : IReplayFeedbackWorkflowIdDestination
{
    public async Task WriteAsync(ReplayFeedbackWorkflowIdRecord record, CancellationToken token)
    {
        await client.ApproveRosterAsync(new ApiScheduling.ApproveRosterRequest { Reason = record.Kind }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentTemplateWorkflowService.</summary>
internal sealed class ReplayFeedbackWorkflowIdDocumentTemplateWorkflowWriter : IReplayFeedbackWorkflowIdDestination
{
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _client;

    public ReplayFeedbackWorkflowIdDocumentTemplateWorkflowWriter(ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(ReplayFeedbackWorkflowIdRecord record, CancellationToken token)
    {
        await _client.ResumeDocumentTemplateWorkflowAsync(new ApiDocuments.ResumeDocumentTemplateWorkflowRequest { DocumentTemplate = new ApiDocuments.DocumentTemplate { Notes = record.Kind, DocumentTemplateId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplayFeedbackWorkflowIdLogOnly : IReplayFeedbackWorkflowIdDestination
{
    public Task WriteAsync(ReplayFeedbackWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.performance.v1.FeedbackWorkflowService/CancelFeedbackWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayFeedbackWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient(channel);
        var response = await source.CancelFeedbackWorkflowAsync(new ApiPerformance.CancelFeedbackWorkflowRequest() { Reason = "primary-replay-feedback-workflow-id" }, cancellationToken: token);
        var record = new ReplayFeedbackWorkflowIdRecord
        {
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
            Kind = Text.Normalize(response.Feedback.Kind),
        };
        await DeliverReplayFeedbackWorkflowIdAsync("corporate-event-workflow", record, channel, token);
    }

    private static async Task DeliverReplayFeedbackWorkflowIdAsync(string kind, ReplayFeedbackWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IReplayFeedbackWorkflowIdDestination sink = new ReplayFeedbackWorkflowIdLogOnly();
        switch (kind)
        {
            case "corporate-event-workflow":
                sink = new ReplayFeedbackWorkflowIdCorporateEventWorkflowSink(new ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient(channel));
                break;
            case "roster-approval":
                sink = new ReplayFeedbackWorkflowIdRosterApprovalWriter(new ApiScheduling.RosterApprovalService.RosterApprovalServiceClient(channel));
                break;
            case "document-template-workflow":
                sink = new ReplayFeedbackWorkflowIdDocumentTemplateWorkflowWriter(new ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient(channel));
                break;
        }
        await sink.WriteAsync(record, token);
    }
}
