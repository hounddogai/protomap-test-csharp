using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where ExportOrgUnitApprovalStatusStatus hands each record.</summary>
internal interface IExportOrgUnitApprovalStatusStatusWriter
{
    Task DeliverAsync(ExportOrgUnitApprovalStatusStatusRecord record, CancellationToken token);
}

/// <summary>What ExportOrgUnitApprovalStatusStatus keeps of each GetOrgUnitApprovalStatus response.</summary>
internal sealed class ExportOrgUnitApprovalStatusStatusRecord
{
    public string Status { get; init; } = "";
    public string OrgUnitId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.benefits.v1.BenefitPlanApprovalService.</summary>
internal sealed class ExportOrgUnitApprovalStatusStatusBenefitPlanApprovalWriter(ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient client) : IExportOrgUnitApprovalStatusStatusWriter
{
    public async Task DeliverAsync(ExportOrgUnitApprovalStatusStatusRecord record, CancellationToken token)
    {
        await client.RecallBenefitPlanAsync(new ApiBenefits.RecallBenefitPlanRequest { BenefitPlan = new ApiBenefits.BenefitPlan { BenefitPlanId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentFolderHistoryService.</summary>
internal sealed class ExportOrgUnitApprovalStatusStatusDocumentFolderHistoryWriter : IExportOrgUnitApprovalStatusStatusWriter
{
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _client;

    public ExportOrgUnitApprovalStatusStatusDocumentFolderHistoryWriter(ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient client) => _client = client;

    public async Task DeliverAsync(ExportOrgUnitApprovalStatusStatusRecord record, CancellationToken token)
    {
        await _client.CompareDocumentFolderVersionsAsync(new ApiDocuments.CompareDocumentFolderVersionsRequest { DocumentFolder = new ApiDocuments.DocumentFolder { ParentId = record.OrgUnitId, DocumentFolderId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitApprovalService/GetOrgUnitApprovalStatus and hands what it keeps to the sink of its kind.</summary>
internal static class ExportOrgUnitApprovalStatusStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient(channel);
        var response = await source.GetOrgUnitApprovalStatusAsync(new ApiEmployees.GetOrgUnitApprovalStatusRequest() { OrgUnitId = "main-export-org-unit-approval-status-status" }, cancellationToken: token);
        var record = new ExportOrgUnitApprovalStatusStatusRecord
        {
            Status = response.Result.Status,
            OrgUnitId = response.OrgUnit.OrgUnitId,
            Message = Text.Normalize(response.Result.Message),
        };
        var sinks = new IExportOrgUnitApprovalStatusStatusWriter[]
        {
            new ExportOrgUnitApprovalStatusStatusBenefitPlanApprovalWriter(new ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient(channel)),
            new ExportOrgUnitApprovalStatusStatusDocumentFolderHistoryWriter(new ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient(channel)),
        };
        await sinks[0].DeliverAsync(record, token);
    }
}
