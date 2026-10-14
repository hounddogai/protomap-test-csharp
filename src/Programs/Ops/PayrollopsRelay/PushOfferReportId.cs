using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where PushOfferReportId hands each record.</summary>
internal interface IPushOfferReportIdSink
{
    Task ForwardAsync(PushOfferReportIdRecord record, CancellationToken token);
}

/// <summary>What PushOfferReportId keeps of each GenerateOfferReport response.</summary>
internal sealed class PushOfferReportIdRecord
{
    public string ReportId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.devices.v1.ComplianceScanWorkflowService.</summary>
internal sealed class PushOfferReportIdComplianceScanWorkflowTarget(ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient client) : IPushOfferReportIdSink
{
    public async Task ForwardAsync(PushOfferReportIdRecord record, CancellationToken token)
    {
        await client.ListComplianceScanWorkflowsAsync(new ApiDevices.ListComplianceScanWorkflowsRequest { Page = new ApiCommon.PageRequest { PageToken = record.ReportId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.timekeeping.v1.OvertimeApprovalService.</summary>
internal sealed class PushOfferReportIdOvertimeApprovalTarget(ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient client) : IPushOfferReportIdSink
{
    public async Task ForwardAsync(PushOfferReportIdRecord record, CancellationToken token)
    {
        await client.GetOvertimeApprovalStatusAsync(new ApiTimekeeping.GetOvertimeApprovalStatusRequest { OvertimeId = record.ReportId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.workplaces.v1.RoomBookingApprovalService.</summary>
internal sealed class PushOfferReportIdRoomBookingApprovalWriter(ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient client) : IPushOfferReportIdSink
{
    public async Task ForwardAsync(PushOfferReportIdRecord record, CancellationToken token)
    {
        await client.GetRoomBookingApprovalStatusAsync(new ApiWorkplaces.GetRoomBookingApprovalStatusRequest { RoomBookingId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushOfferReportIdLogOnly : IPushOfferReportIdSink
{
    public Task ForwardAsync(PushOfferReportIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReportId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.recruiting.v1.OfferReportingService/GenerateOfferReport and hands what it keeps to the sink of its kind.</summary>
internal static class PushOfferReportId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.OfferReportingService.OfferReportingServiceClient(channel);
        var found = await source.GenerateOfferReportAsync(new ApiRecruiting.GenerateOfferReportRequest() { OfferId = "current-push-offer-report-id" }, cancellationToken: token);
        var record = new PushOfferReportIdRecord
        {
            ReportId = found.ReportId,
            Status = Text.Normalize(found.Result.Status),
        };
        await DispatchPushOfferReportIdAsync("compliance-scan-workflow", record, channel, token);
    }

    private static async Task DispatchPushOfferReportIdAsync(string kind, PushOfferReportIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<PushOfferReportIdRecord, CancellationToken, Task> write = new PushOfferReportIdLogOnly().ForwardAsync;
        switch (kind)
        {
            case "compliance-scan-workflow":
                write = new PushOfferReportIdComplianceScanWorkflowTarget(new ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient(channel)).ForwardAsync;
                break;
            case "overtime-approval":
                write = new PushOfferReportIdOvertimeApprovalTarget(new ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient(channel)).ForwardAsync;
                break;
            case "room-booking-approval":
                write = new PushOfferReportIdRoomBookingApprovalWriter(new ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient(channel)).ForwardAsync;
                break;
        }
        await write(record, token);
    }
}
