using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ExportTimesheetsStatus hands each record.</summary>
internal interface IExportTimesheetsStatusDestination
{
    Task SendAsync(ExportTimesheetsStatusRecord record, CancellationToken token);
}

/// <summary>What ExportTimesheetsStatus keeps of each SummarizeTimesheets response.</summary>
internal sealed class ExportTimesheetsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.scheduling.v1.AvailabilityWindowWorkflowService.</summary>
internal sealed class ExportTimesheetsStatusAvailabilityWindowWorkflowRelay : IExportTimesheetsStatusDestination
{
    private readonly ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient _client;

    public ExportTimesheetsStatusAvailabilityWindowWorkflowRelay(ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(ExportTimesheetsStatusRecord record, CancellationToken token)
    {
        await _client.ListAvailabilityWindowWorkflowsAsync(new ApiScheduling.ListAvailabilityWindowWorkflowsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.FeedbackApprovalService.</summary>
internal sealed class ExportTimesheetsStatusFeedbackApprovalWriter : IExportTimesheetsStatusDestination
{
    private readonly ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient _client;

    public ExportTimesheetsStatusFeedbackApprovalWriter(ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient client) => _client = client;

    public async Task SendAsync(ExportTimesheetsStatusRecord record, CancellationToken token)
    {
        await _client.DelegateFeedbackApprovalAsync(new ApiPerformance.DelegateFeedbackApprovalRequest { Details = new ApiPerformance.DelegateFeedbackApprovalRequest.Types.Details { ApproverEmail = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.benefits.v1.BenefitPlanApprovalService.</summary>
internal sealed class ExportTimesheetsStatusBenefitPlanApprovalTarget(ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient client) : IExportTimesheetsStatusDestination
{
    public async Task SendAsync(ExportTimesheetsStatusRecord record, CancellationToken token)
    {
        await client.EscalateBenefitPlanAsync(new ApiBenefits.EscalateBenefitPlanRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.timekeeping.v1.TimesheetQueryService/SummarizeTimesheets and hands what it keeps to the sink of its kind.</summary>
internal static class ExportTimesheetsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient(channel);
        var got = await source.SummarizeTimesheetsAsync(new ApiTimekeeping.SummarizeTimesheetsRequest() { TimesheetId = "live-export-timesheets-status" }, cancellationToken: token);
        var record = new ExportTimesheetsStatusRecord
        {
            Status = got.Result.Status,
            Message = got.Result.Message,
        };
        var sinks = new List<IExportTimesheetsStatusDestination>
        {
            new ExportTimesheetsStatusAvailabilityWindowWorkflowRelay(new ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient(channel)),
            new ExportTimesheetsStatusFeedbackApprovalWriter(new ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient(channel)),
            new ExportTimesheetsStatusBenefitPlanApprovalTarget(new ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient(channel)),
        };
        await sinks[2].SendAsync(record, token);
    }
}
