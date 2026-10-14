using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiDevices = Acme.Devices.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where ExportBoxOfficeSaleNotes hands each record.</summary>
internal interface IExportBoxOfficeSaleNotesSink
{
    Task WriteAsync(ExportBoxOfficeSaleNotesRecord record, CancellationToken token);
}

/// <summary>What ExportBoxOfficeSaleNotes keeps of each UpdateBoxOfficeSale response.</summary>
internal sealed class ExportBoxOfficeSaleNotesRecord
{
    public string Notes { get; init; } = "";
    public string BoxOfficeSaleId { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Sends records to acme.benefits.v1.BenefitPlanWorkflowService.</summary>
internal sealed class ExportBoxOfficeSaleNotesBenefitPlanWorkflowSink(ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient client) : IExportBoxOfficeSaleNotesSink
{
    public async Task WriteAsync(ExportBoxOfficeSaleNotesRecord record, CancellationToken token)
    {
        await client.CancelBenefitPlanWorkflowAsync(new ApiBenefits.CancelBenefitPlanWorkflowRequest { BenefitPlan = new ApiBenefits.BenefitPlan { Name = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.ComplianceScanApprovalService.</summary>
internal sealed class ExportBoxOfficeSaleNotesComplianceScanApprovalRelay : IExportBoxOfficeSaleNotesSink
{
    private readonly ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient _client;

    public ExportBoxOfficeSaleNotesComplianceScanApprovalRelay(ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(ExportBoxOfficeSaleNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.ApproveComplianceScanRequest { ComplianceScan = new ApiDevices.ComplianceScan { Source = record.Number, ComplianceScanId = record.BoxOfficeSaleId } }, token, _client.ApproveComplianceScanAsync);
    }
}

/// <summary>Reads acme.boxoffice.v1.BoxOfficeSaleService/UpdateBoxOfficeSale and hands what it keeps to the sink of its kind.</summary>
internal static class ExportBoxOfficeSaleNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient(channel);
        var got = await source.UpdateBoxOfficeSaleAsync(new ApiBoxoffice.UpdateBoxOfficeSaleRequest(), cancellationToken: token);
        var record = new ExportBoxOfficeSaleNotesRecord
        {
            Notes = got.BoxOfficeSale.Notes,
            BoxOfficeSaleId = Text.Normalize(got.BoxOfficeSale.BoxOfficeSaleId),
            Number = got.BoxOfficeSale.Number,
        };
        var sinks = new Dictionary<string, IExportBoxOfficeSaleNotesSink>
        {
            ["benefit-plan-workflow"] = new ExportBoxOfficeSaleNotesBenefitPlanWorkflowSink(new ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient(channel)),
            ["compliance-scan-approval"] = new ExportBoxOfficeSaleNotesComplianceScanApprovalRelay(new ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("EXPORT_BOX_OFFICE_SALE_NOTES_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].WriteAsync(record, token);
    }
}
