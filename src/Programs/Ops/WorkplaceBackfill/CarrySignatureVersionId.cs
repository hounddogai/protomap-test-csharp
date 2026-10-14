using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where CarrySignatureVersionId hands each record.</summary>
internal interface ICarrySignatureVersionIdWriter
{
    Task WriteAsync(CarrySignatureVersionIdRecord record, CancellationToken token);
}

/// <summary>What CarrySignatureVersionId keeps of each AnnotateSignatureVersion response.</summary>
internal sealed class CarrySignatureVersionIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string SignatureId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.workplaces.v1.VisitorPassCalculationService.</summary>
internal sealed class CarrySignatureVersionIdVisitorPassCalculationSink(ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient client) : ICarrySignatureVersionIdWriter
{
    public async Task WriteAsync(CarrySignatureVersionIdRecord record, CancellationToken token)
    {
        await client.EstimateVisitorPassAsync(new ApiWorkplaces.EstimateVisitorPassRequest { VisitorPass = new ApiWorkplaces.VisitorPass { Notes = record.Status, VisitorPassId = record.SignatureId }, VisitorPassId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.scheduling.v1.ShiftQueryService.</summary>
internal sealed class CarrySignatureVersionIdShiftQueryTarget : ICarrySignatureVersionIdWriter
{
    private readonly ApiScheduling.ShiftQueryService.ShiftQueryServiceClient _client;

    public CarrySignatureVersionIdShiftQueryTarget(ApiScheduling.ShiftQueryService.ShiftQueryServiceClient client) => _client = client;

    public async Task WriteAsync(CarrySignatureVersionIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.FindShiftsRequest { Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, token, _client.FindShiftsAsync);
    }
}

/// <summary>Passes records to acme.employees.v1.OrgUnitWorkflowService.</summary>
internal sealed class CarrySignatureVersionIdOrgUnitWorkflowRelay : ICarrySignatureVersionIdWriter
{
    private readonly ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient _client;

    public CarrySignatureVersionIdOrgUnitWorkflowRelay(ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(CarrySignatureVersionIdRecord record, CancellationToken token)
    {
        await _client.AdvanceOrgUnitWorkflowAsync(new ApiEmployees.AdvanceOrgUnitWorkflowRequest { Details = new ApiEmployees.AdvanceOrgUnitWorkflowRequest.Types.Details { Step = record.SignatureId }, OrgUnitId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.InterviewSyncService.</summary>
internal sealed class CarrySignatureVersionIdInterviewSyncWriter(ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient client) : ICarrySignatureVersionIdWriter
{
    public async Task WriteAsync(CarrySignatureVersionIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.ListInterviewConflictsRequest { OrderBy = record.CorrelationId, Filter = new ApiRecruiting.ListInterviewConflictsRequest.Types.Filter { Query = record.SignatureId } }, token, client.ListInterviewConflictsAsync);
    }
}

/// <summary>Reads acme.documents.v1.SignatureHistoryService/AnnotateSignatureVersion and hands what it keeps to the sink of its kind.</summary>
internal static class CarrySignatureVersionId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient(channel);
        var found = await source.AnnotateSignatureVersionAsync(new ApiDocuments.AnnotateSignatureVersionRequest() { Reason = "current-carry-signature-version-id" }, cancellationToken: token);
        var record = new CarrySignatureVersionIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            SignatureId = found.Signature.SignatureId,
            Status = Text.Normalize(found.Outcome.Status),
        };
        var sinks = new Dictionary<string, ICarrySignatureVersionIdWriter>
        {
            ["visitor-pass-calculation"] = new CarrySignatureVersionIdVisitorPassCalculationSink(new ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient(channel)),
            ["shift-query"] = new CarrySignatureVersionIdShiftQueryTarget(new ApiScheduling.ShiftQueryService.ShiftQueryServiceClient(channel)),
            ["org-unit-workflow"] = new CarrySignatureVersionIdOrgUnitWorkflowRelay(new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel)),
            ["interview-sync"] = new CarrySignatureVersionIdInterviewSyncWriter(new ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("CARRY_SIGNATURE_VERSION_ID_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].WriteAsync(record, token);
    }
}
