using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where BackfillDriverId hands each record.</summary>
internal interface IBackfillDriverIdDestination
{
    Task ForwardAsync(BackfillDriverIdRecord record, CancellationToken token);
}

/// <summary>What BackfillDriverId keeps of each EstimateDriver response.</summary>
internal sealed class BackfillDriverIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string DriverId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.helpdesk.v1.TicketWorkflowService.</summary>
internal sealed class BackfillDriverIdTicketWorkflowTarget : IBackfillDriverIdDestination
{
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _client;

    public BackfillDriverIdTicketWorkflowTarget(ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillDriverIdRecord record, CancellationToken token)
    {
        await _client.StartTicketWorkflowAsync(new ApiHelpdesk.StartTicketWorkflowRequest { Details = new ApiHelpdesk.StartTicketWorkflowRequest.Types.Details { InitiatedBy = record.DriverId }, Ticket = new ApiHelpdesk.Ticket { Subject = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.devices.v1.ComplianceScanApprovalService.</summary>
internal sealed class BackfillDriverIdComplianceScanApprovalTarget(ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient client) : IBackfillDriverIdDestination
{
    public async Task ForwardAsync(BackfillDriverIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.ApproveComplianceScanRequest { ComplianceScan = new ApiDevices.ComplianceScan { Kind = record.Status }, Details = new ApiDevices.ApproveComplianceScanRequest.Types.Details { ApproverEmail = record.CorrelationId } }, token, client.ApproveComplianceScanAsync);
    }
}

/// <summary>Hands records to acme.learning.v1.CourseWorkflowService.</summary>
internal sealed class BackfillDriverIdCourseWorkflowRelay : IBackfillDriverIdDestination
{
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _client;

    public BackfillDriverIdCourseWorkflowRelay(ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillDriverIdRecord record, CancellationToken token)
    {
        await _client.ListCourseWorkflowsAsync(new ApiLearning.ListCourseWorkflowsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiLearning.ListCourseWorkflowsRequest.Types.Filter { Region = record.DriverId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.recruiting.v1.JobOpeningWorkflowService.</summary>
internal sealed class BackfillDriverIdJobOpeningWorkflowRelay : IBackfillDriverIdDestination
{
    private readonly ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient _client;

    public BackfillDriverIdJobOpeningWorkflowRelay(ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillDriverIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.PauseJobOpeningWorkflowRequest { JobOpening = new ApiRecruiting.JobOpening { Notes = record.Status, JobOpeningId = record.CorrelationId }, JobOpeningId = record.DriverId }, token, _client.PauseJobOpeningWorkflowAsync);
    }
}

/// <summary>Reads acme.drivers.v1.DriverCalculationService/EstimateDriver and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillDriverId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDrivers.DriverCalculationService.DriverCalculationServiceClient(channel);
        var response = await source.EstimateDriverAsync(new ApiDrivers.EstimateDriverRequest() { Reason = "nightly-backfill-driver-id" }, cancellationToken: token);
        var record = new BackfillDriverIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            DriverId = response.Driver.DriverId,
            Status = response.Result.Status,
        };
        var sinks = new Dictionary<string, IBackfillDriverIdDestination>
        {
            ["ticket-workflow"] = new BackfillDriverIdTicketWorkflowTarget(new ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient(channel)),
            ["compliance-scan-approval"] = new BackfillDriverIdComplianceScanApprovalTarget(new ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient(channel)),
            ["course-workflow"] = new BackfillDriverIdCourseWorkflowRelay(new ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient(channel)),
            ["job-opening-workflow"] = new BackfillDriverIdJobOpeningWorkflowRelay(new ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "course-workflow")
                continue;
            await sink.ForwardAsync(record, token);
        }
    }
}
