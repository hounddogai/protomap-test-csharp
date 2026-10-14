using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where SyncKnowledgeArticleApprovalNumber hands each record.</summary>
internal interface ISyncKnowledgeArticleApprovalNumberSink
{
    Task DeliverAsync(SyncKnowledgeArticleApprovalNumberRecord record, CancellationToken token);
}

/// <summary>What SyncKnowledgeArticleApprovalNumber keeps of each CommentKnowledgeArticleApproval response.</summary>
internal sealed class SyncKnowledgeArticleApprovalNumberRecord
{
    public string Number { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.devices.v1.ComplianceScanCalculationService.</summary>
internal sealed class SyncKnowledgeArticleApprovalNumberComplianceScanCalculationRelay : ISyncKnowledgeArticleApprovalNumberSink
{
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _client;

    public SyncKnowledgeArticleApprovalNumberComplianceScanCalculationRelay(ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncKnowledgeArticleApprovalNumberRecord record, CancellationToken token)
    {
        await _client.CalculateComplianceScanAsync(new ApiDevices.CalculateComplianceScanRequest { Details = new ApiDevices.CalculateComplianceScanRequest.Types.Details { CurrencyCode = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.EquipmentApprovalService.</summary>
internal sealed class SyncKnowledgeArticleApprovalNumberEquipmentApprovalSink(ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient client) : ISyncKnowledgeArticleApprovalNumberSink
{
    public async Task DeliverAsync(SyncKnowledgeArticleApprovalNumberRecord record, CancellationToken token)
    {
        await client.ListPendingEquipmentsAsync(new ApiOnboarding.ListPendingEquipmentsRequest { OrderBy = record.Notes, Filter = new ApiOnboarding.ListPendingEquipmentsRequest.Types.Filter { Query = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.helpdesk.v1.KnowledgeArticleApprovalService/CommentKnowledgeArticleApproval and hands what it keeps to the sink of its kind.</summary>
internal static class SyncKnowledgeArticleApprovalNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient(channel);
        var found = await source.CommentKnowledgeArticleApprovalAsync(new ApiHelpdesk.CommentKnowledgeArticleApprovalRequest() { KnowledgeArticleId = "live-sync-knowledge-article-approval-number" }, cancellationToken: token);
        var record = new SyncKnowledgeArticleApprovalNumberRecord
        {
            Number = found.KnowledgeArticle.Number,
            Notes = found.KnowledgeArticle.Notes,
            Status = found.Result.Status,
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
        };
        var sinks = new List<ISyncKnowledgeArticleApprovalNumberSink>
        {
            new SyncKnowledgeArticleApprovalNumberComplianceScanCalculationRelay(new ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient(channel)),
            new SyncKnowledgeArticleApprovalNumberEquipmentApprovalSink(new ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.DeliverAsync(record, token);
    }
}
