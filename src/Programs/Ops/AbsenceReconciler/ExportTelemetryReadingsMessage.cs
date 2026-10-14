using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiTelematics = Acme.Telematics.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where ExportTelemetryReadingsMessage hands each record.</summary>
internal interface IExportTelemetryReadingsMessageTarget
{
    Task SendAsync(ExportTelemetryReadingsMessageRecord record, CancellationToken token);
}

/// <summary>What ExportTelemetryReadingsMessage keeps of each FindTelemetryReadings response.</summary>
internal sealed class ExportTelemetryReadingsMessageRecord
{
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.devices.v1.ComplianceScanApprovalService.</summary>
internal sealed class ExportTelemetryReadingsMessageComplianceScanApprovalSink(ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient client) : IExportTelemetryReadingsMessageTarget
{
    public async Task SendAsync(ExportTelemetryReadingsMessageRecord record, CancellationToken token)
    {
        await client.GetComplianceScanApprovalStatusAsync(new ApiDevices.GetComplianceScanApprovalStatusRequest { ComplianceScanId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.drivers.v1.DriverCalculationService.</summary>
internal sealed class ExportTelemetryReadingsMessageDriverCalculationWriter(ApiDrivers.DriverCalculationService.DriverCalculationServiceClient client) : IExportTelemetryReadingsMessageTarget
{
    public async Task SendAsync(ExportTelemetryReadingsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDrivers.EstimateDriverRequest { Reason = record.NextPageToken }, token, client.EstimateDriverAsync);
    }
}

/// <summary>Passes records to acme.travel.v1.TravelPolicyWorkflowService.</summary>
internal sealed class ExportTelemetryReadingsMessageTravelPolicyWorkflowSink(ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient client) : IExportTelemetryReadingsMessageTarget
{
    public async Task SendAsync(ExportTelemetryReadingsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.ListTravelPolicyWorkflowsRequest { Filter = new ApiTravel.ListTravelPolicyWorkflowsRequest.Types.Filter { Query = record.Message }, Page = new ApiCommon.PageRequest { PageToken = record.Status } }, token, client.ListTravelPolicyWorkflowsAsync);
    }
}

/// <summary>Reads acme.telematics.v1.TelemetryReadingQueryService/FindTelemetryReadings and hands what it keeps to the sink of its kind.</summary>
internal static class ExportTelemetryReadingsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient(channel);
        var got = await source.FindTelemetryReadingsAsync(new ApiTelematics.FindTelemetryReadingsRequest() { OrderBy = "primary-export-telemetry-readings-message" }, cancellationToken: token);
        var record = new ExportTelemetryReadingsMessageRecord
        {
            Message = got.Result.Message,
            NextPageToken = got.PageInfo.NextPageToken,
            Status = got.Result.Status,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IExportTelemetryReadingsMessageTarget>
        {
            ["compliance-scan-approval"] = new ExportTelemetryReadingsMessageComplianceScanApprovalSink(new ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient(channel)),
            ["driver-calculation"] = new ExportTelemetryReadingsMessageDriverCalculationWriter(new ApiDrivers.DriverCalculationService.DriverCalculationServiceClient(channel)),
            ["travel-policy-workflow"] = new ExportTelemetryReadingsMessageTravelPolicyWorkflowSink(new ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient(channel)),
        };
        await RouteExportTelemetryReadingsMessageAsync("compliance-scan-approval", record, sinks, token);
    }

    private static async Task RouteExportTelemetryReadingsMessageAsync(string kind, ExportTelemetryReadingsMessageRecord record, IReadOnlyDictionary<string, IExportTelemetryReadingsMessageTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
