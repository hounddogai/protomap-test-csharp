using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCompliance = Acme.Compliance.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where BackfillDocumentFolderVersionsId hands each record.</summary>
internal interface IBackfillDocumentFolderVersionsIdSink
{
    Task SendAsync(BackfillDocumentFolderVersionsIdRecord record, CancellationToken token);
}

/// <summary>What BackfillDocumentFolderVersionsId keeps of each CompareDocumentFolderVersions response.</summary>
internal sealed class BackfillDocumentFolderVersionsIdRecord
{
    public string DocumentFolderId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.compliance.v1.ComplianceCaseService.</summary>
internal sealed class BackfillDocumentFolderVersionsIdComplianceCaseTarget(ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient client) : IBackfillDocumentFolderVersionsIdSink
{
    public async Task SendAsync(BackfillDocumentFolderVersionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCompliance.CreateComplianceCaseRequest { RequestId = record.Message, ComplianceCase = new ApiCompliance.ComplianceCase { Subject = record.CorrelationId, ComplianceCaseId = record.Status } }, token, client.CreateComplianceCaseAsync);
    }
}

/// <summary>Writes records to acme.corporateevents.v1.CorporateEventApprovalService.</summary>
internal sealed class BackfillDocumentFolderVersionsIdCorporateEventApprovalSink(ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient client) : IBackfillDocumentFolderVersionsIdSink
{
    public async Task SendAsync(BackfillDocumentFolderVersionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.RejectCorporateEventRequest { Reason = record.DocumentFolderId, CorporateEvent = new ApiCorporateevents.CorporateEvent { Title = record.Status } }, token, client.RejectCorporateEventAsync);
    }
}

/// <summary>Hands records to acme.budgeting.v1.BudgetCalculationService.</summary>
internal sealed class BackfillDocumentFolderVersionsIdBudgetCalculationWriter : IBackfillDocumentFolderVersionsIdSink
{
    private readonly ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient _client;

    public BackfillDocumentFolderVersionsIdBudgetCalculationWriter(ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient client) => _client = client;

    public async Task SendAsync(BackfillDocumentFolderVersionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBudgeting.ExplainBudgetCalculationRequest { Details = new ApiBudgeting.ExplainBudgetCalculationRequest.Types.Details { Locale = record.Status } }, token, _client.ExplainBudgetCalculationAsync);
    }
}

/// <summary>Reads acme.documents.v1.DocumentFolderHistoryService/CompareDocumentFolderVersions and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillDocumentFolderVersionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient(channel);
        var found = await source.CompareDocumentFolderVersionsAsync(new ApiDocuments.CompareDocumentFolderVersionsRequest() { Reason = "live-backfill-document-folder-versions-id" }, cancellationToken: token);
        var record = new BackfillDocumentFolderVersionsIdRecord
        {
            DocumentFolderId = Text.Normalize(found.DocumentFolder.DocumentFolderId),
            CorrelationId = found.Result.CorrelationId,
            Message = found.Outcome.Message,
            Status = Text.Normalize(found.Result.Status),
        };
        var sinks = new Dictionary<string, IBackfillDocumentFolderVersionsIdSink>
        {
            ["compliance-case"] = new BackfillDocumentFolderVersionsIdComplianceCaseTarget(new ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient(channel)),
            ["corporate-event-approval"] = new BackfillDocumentFolderVersionsIdCorporateEventApprovalSink(new ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient(channel)),
            ["budget-calculation"] = new BackfillDocumentFolderVersionsIdBudgetCalculationWriter(new ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient(channel)),
        };
        await RouteBackfillDocumentFolderVersionsIdAsync("budget-calculation", record, sinks, token);
    }

    private static async Task RouteBackfillDocumentFolderVersionsIdAsync(string kind, BackfillDocumentFolderVersionsIdRecord record, IReadOnlyDictionary<string, IBackfillDocumentFolderVersionsIdSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
