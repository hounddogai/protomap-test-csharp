using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where ExportClockEventId hands each record.</summary>
internal interface IExportClockEventIdTarget
{
    Task ApplyAsync(ExportClockEventIdRecord record, CancellationToken token);
}

/// <summary>What ExportClockEventId keeps of each SubmitClockEvent response.</summary>
internal sealed class ExportClockEventIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.personalization.v1.ProfileService.</summary>
internal sealed class ExportClockEventIdProfileSink(ApiPersonalization.ProfileService.ProfileServiceClient client) : IExportClockEventIdTarget
{
    public async Task ApplyAsync(ExportClockEventIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPersonalization.CloneProfileRequest { Profile = new ApiPersonalization.Profile { ProfileId = record.Message }, RequestId = record.CorrelationId }, token, client.CloneProfileAsync);
    }
}

/// <summary>Sends records to acme.documents.v1.DocumentTemplateWorkflowService.</summary>
internal sealed class ExportClockEventIdDocumentTemplateWorkflowRelay : IExportClockEventIdTarget
{
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _client;

    public ExportClockEventIdDocumentTemplateWorkflowRelay(ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient client) => _client = client;

    public async Task ApplyAsync(ExportClockEventIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.InspectDocumentTemplateWorkflowRequest { DocumentTemplateId = record.ReferenceId }, token, _client.InspectDocumentTemplateWorkflowAsync);
    }
}

/// <summary>Sends records to acme.fanportal.v1.FanSessionQueryService.</summary>
internal sealed class ExportClockEventIdFanSessionQueryTarget : IExportClockEventIdTarget
{
    private readonly ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient _client;

    public ExportClockEventIdFanSessionQueryTarget(ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(ExportClockEventIdRecord record, CancellationToken token)
    {
        await _client.LookupFanSessionAsync(new ApiFanportal.LookupFanSessionRequest { FanSessionId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.devices.v1.ComplianceScanValidationService.</summary>
internal sealed class ExportClockEventIdComplianceScanValidationRelay(ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient client) : IExportClockEventIdTarget
{
    public async Task ApplyAsync(ExportClockEventIdRecord record, CancellationToken token)
    {
        await client.ExplainComplianceScanDecisionAsync(new ApiDevices.ExplainComplianceScanDecisionRequest { ComplianceScanId = record.CorrelationId, Reason = record.Message, ComplianceScan = new ApiDevices.ComplianceScan { ComplianceScanId = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.timekeeping.v1.ClockEventApprovalService/SubmitClockEvent and hands what it keeps to the sink of its kind.</summary>
internal static class ExportClockEventId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient(channel);
        var got = await source.SubmitClockEventAsync(new ApiTimekeeping.SubmitClockEventRequest() { Reason = "main-export-clock-event-id" }, cancellationToken: token);
        var record = new ExportClockEventIdRecord
        {
            CorrelationId = Text.Normalize(got.ClockEvent.CorrelationId),
            ReferenceId = got.Outcome.ReferenceId,
            Message = got.Result.Message,
        };
        var sinks = new Dictionary<string, IExportClockEventIdTarget>
        {
            ["profile"] = new ExportClockEventIdProfileSink(new ApiPersonalization.ProfileService.ProfileServiceClient(channel)),
            ["document-template-workflow"] = new ExportClockEventIdDocumentTemplateWorkflowRelay(new ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient(channel)),
            ["fan-session-query"] = new ExportClockEventIdFanSessionQueryTarget(new ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient(channel)),
            ["compliance-scan-validation"] = new ExportClockEventIdComplianceScanValidationRelay(new ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient(channel)),
        };
        await RouteExportClockEventIdAsync("document-template-workflow", record, sinks, token);
    }

    private static async Task RouteExportClockEventIdAsync(string kind, ExportClockEventIdRecord record, IReadOnlyDictionary<string, IExportClockEventIdTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
