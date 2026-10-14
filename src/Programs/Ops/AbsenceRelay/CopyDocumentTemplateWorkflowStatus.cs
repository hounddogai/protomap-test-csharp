using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTelematics = Acme.Telematics.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where CopyDocumentTemplateWorkflowStatus hands each record.</summary>
internal interface ICopyDocumentTemplateWorkflowStatusTarget
{
    Task ForwardAsync(CopyDocumentTemplateWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What CopyDocumentTemplateWorkflowStatus keeps of each CancelDocumentTemplateWorkflow response.</summary>
internal sealed class CopyDocumentTemplateWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
}

/// <summary>Sends records to acme.itsm.v1.ServiceAssetApprovalService.</summary>
internal sealed class CopyDocumentTemplateWorkflowStatusServiceAssetApprovalWriter(ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient client) : ICopyDocumentTemplateWorkflowStatusTarget
{
    public async Task ForwardAsync(CopyDocumentTemplateWorkflowStatusRecord record, CancellationToken token)
    {
        await client.SubmitServiceAssetAsync(new ApiItsm.SubmitServiceAssetRequest { ServiceAssetId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.telematics.v1.TelemetryReadingQueryService.</summary>
internal sealed class CopyDocumentTemplateWorkflowStatusTelemetryReadingQueryRelay : ICopyDocumentTemplateWorkflowStatusTarget
{
    private readonly ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient _client;

    public CopyDocumentTemplateWorkflowStatusTelemetryReadingQueryRelay(ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(CopyDocumentTemplateWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTelematics.BrowseTelemetryReadingsRequest { Filter = new ApiTelematics.BrowseTelemetryReadingsRequest.Types.Filter { Region = record.Status, OwnerId = record.OutcomeStatus }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, token, _client.BrowseTelemetryReadingsAsync);
    }
}

/// <summary>Hands records to acme.warehousing.v1.WarehouseQueryService.</summary>
internal sealed class CopyDocumentTemplateWorkflowStatusWarehouseQuerySink : ICopyDocumentTemplateWorkflowStatusTarget
{
    private readonly ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient _client;

    public CopyDocumentTemplateWorkflowStatusWarehouseQuerySink(ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(CopyDocumentTemplateWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWarehousing.SummarizeWarehousesRequest { Details = new ApiWarehousing.SummarizeWarehousesRequest.Types.Details { Format = record.CorrelationId }, WarehouseId = record.Status }, token, _client.SummarizeWarehousesAsync);
    }
}

/// <summary>Writes records to acme.employees.v1.OrgUnitApprovalService.</summary>
internal sealed class CopyDocumentTemplateWorkflowStatusOrgUnitApprovalSink(ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient client) : ICopyDocumentTemplateWorkflowStatusTarget
{
    public async Task ForwardAsync(CopyDocumentTemplateWorkflowStatusRecord record, CancellationToken token)
    {
        await client.RecallOrgUnitAsync(new ApiEmployees.RecallOrgUnitRequest { Details = new ApiEmployees.RecallOrgUnitRequest.Types.Details { ApproverEmail = record.Status }, OrgUnitId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CopyDocumentTemplateWorkflowStatusLogOnly : ICopyDocumentTemplateWorkflowStatusTarget
{
    public Task ForwardAsync(CopyDocumentTemplateWorkflowStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.DocumentTemplateWorkflowService/CancelDocumentTemplateWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class CopyDocumentTemplateWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient(channel);
        var reply = await source.CancelDocumentTemplateWorkflowAsync(new ApiDocuments.CancelDocumentTemplateWorkflowRequest() { Reason = "default-copy-document-template-workflow-status" }, cancellationToken: token);
        var record = new CopyDocumentTemplateWorkflowStatusRecord
        {
            Status = reply.Result.Status,
            CorrelationId = reply.Result.CorrelationId,
            OutcomeStatus = reply.Outcome.Status,
        };
        await DeliverCopyDocumentTemplateWorkflowStatusAsync("warehouse-query", record, channel, token);
    }

    private static async Task DeliverCopyDocumentTemplateWorkflowStatusAsync(string kind, CopyDocumentTemplateWorkflowStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        ICopyDocumentTemplateWorkflowStatusTarget sink = new CopyDocumentTemplateWorkflowStatusLogOnly();
        switch (kind)
        {
            case "asset-approval":
                sink = new CopyDocumentTemplateWorkflowStatusServiceAssetApprovalWriter(new ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient(channel));
                break;
            case "telemetry-reading-query":
                sink = new CopyDocumentTemplateWorkflowStatusTelemetryReadingQueryRelay(new ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient(channel));
                break;
            case "warehouse-query":
                sink = new CopyDocumentTemplateWorkflowStatusWarehouseQuerySink(new ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient(channel));
                break;
            case "org-unit-approval":
                sink = new CopyDocumentTemplateWorkflowStatusOrgUnitApprovalSink(new ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
