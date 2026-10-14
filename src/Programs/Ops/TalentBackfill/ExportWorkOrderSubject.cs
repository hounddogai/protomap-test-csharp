using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMaintenance = Acme.Maintenance.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where ExportWorkOrderSubject hands each record.</summary>
internal interface IExportWorkOrderSubjectDestination
{
    Task ForwardAsync(ExportWorkOrderSubjectRecord record, CancellationToken token);
}

/// <summary>What ExportWorkOrderSubject keeps of each QuoteWorkOrder response.</summary>
internal sealed class ExportWorkOrderSubjectRecord
{
    public string Subject { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string WorkOrderId { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentTemplateExportService.</summary>
internal sealed class ExportWorkOrderSubjectDocumentTemplateExportTarget : IExportWorkOrderSubjectDestination
{
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _client;

    public ExportWorkOrderSubjectDocumentTemplateExportTarget(ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient client) => _client = client;

    public async Task ForwardAsync(ExportWorkOrderSubjectRecord record, CancellationToken token)
    {
        await _client.ListDocumentTemplateExportsAsync(new ApiDocuments.ListDocumentTemplateExportsRequest { OrderBy = record.Message, Filter = new ApiDocuments.ListDocumentTemplateExportsRequest.Types.Filter { Query = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.keymanagement.v1.CryptoKeyCalculationService.</summary>
internal sealed class ExportWorkOrderSubjectCryptoKeyCalculationTarget : IExportWorkOrderSubjectDestination
{
    private readonly ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient _client;

    public ExportWorkOrderSubjectCryptoKeyCalculationTarget(ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(ExportWorkOrderSubjectRecord record, CancellationToken token)
    {
        await _client.ExplainCryptoKeyCalculationAsync(new ApiKeymanagement.ExplainCryptoKeyCalculationRequest { Details = new ApiKeymanagement.ExplainCryptoKeyCalculationRequest.Types.Details { Locale = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.learning.v1.LearningPathApprovalService.</summary>
internal sealed class ExportWorkOrderSubjectLearningPathApprovalSink : IExportWorkOrderSubjectDestination
{
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _client;

    public ExportWorkOrderSubjectLearningPathApprovalSink(ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(ExportWorkOrderSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.DelegateLearningPathApprovalRequest { Reason = record.WorkOrderId, LearningPath = new ApiLearning.LearningPath { Name = record.Subject } }, token, _client.DelegateLearningPathApprovalAsync);
    }
}

/// <summary>Writes records to acme.employeeportal.v1.AnnouncementWorkflowService.</summary>
internal sealed class ExportWorkOrderSubjectAnnouncementWorkflowRelay : IExportWorkOrderSubjectDestination
{
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _client;

    public ExportWorkOrderSubjectAnnouncementWorkflowRelay(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(ExportWorkOrderSubjectRecord record, CancellationToken token)
    {
        await _client.StartAnnouncementWorkflowAsync(new ApiEmployeeportal.StartAnnouncementWorkflowRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.maintenance.v1.WorkOrderCalculationService/QuoteWorkOrder and hands what it keeps to the sink of its kind.</summary>
internal static class ExportWorkOrderSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient(channel);
        var found = await source.QuoteWorkOrderAsync(new ApiMaintenance.QuoteWorkOrderRequest() { WorkOrderId = "default-export-work-order-subject" }, cancellationToken: token);
        var record = new ExportWorkOrderSubjectRecord
        {
            Subject = found.WorkOrder.Subject,
            Message = found.Result.Message,
            Status = found.Result.Status,
            WorkOrderId = found.WorkOrder.WorkOrderId,
        };
        var sinks = new Dictionary<string, IExportWorkOrderSubjectDestination>
        {
            ["document-template-export"] = new ExportWorkOrderSubjectDocumentTemplateExportTarget(new ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient(channel)),
            ["crypto-key-calculation"] = new ExportWorkOrderSubjectCryptoKeyCalculationTarget(new ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient(channel)),
            ["learning-path-approval"] = new ExportWorkOrderSubjectLearningPathApprovalSink(new ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient(channel)),
            ["announcement-workflow"] = new ExportWorkOrderSubjectAnnouncementWorkflowRelay(new ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient(channel)),
        };
        await RouteExportWorkOrderSubjectAsync("crypto-key-calculation", record, sinks, token);
    }

    private static async Task RouteExportWorkOrderSubjectAsync(string kind, ExportWorkOrderSubjectRecord record, IReadOnlyDictionary<string, IExportWorkOrderSubjectDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
