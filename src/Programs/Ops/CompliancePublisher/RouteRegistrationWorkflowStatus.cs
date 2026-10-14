using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where RouteRegistrationWorkflowStatus hands each record.</summary>
internal interface IRouteRegistrationWorkflowStatusWriter
{
    Task DeliverAsync(RouteRegistrationWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What RouteRegistrationWorkflowStatus keeps of each PauseRegistrationWorkflow response.</summary>
internal sealed class RouteRegistrationWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.scheduling.v1.AvailabilityWindowApprovalService.</summary>
internal sealed class RouteRegistrationWorkflowStatusAvailabilityWindowApprovalTarget : IRouteRegistrationWorkflowStatusWriter
{
    private readonly ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient _client;

    public RouteRegistrationWorkflowStatusAvailabilityWindowApprovalTarget(ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(RouteRegistrationWorkflowStatusRecord record, CancellationToken token)
    {
        await _client.DelegateAvailabilityWindowApprovalAsync(new ApiScheduling.DelegateAvailabilityWindowApprovalRequest { AvailabilityWindow = new ApiScheduling.AvailabilityWindow { Title = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.itsm.v1.ServiceAssetReportingService.</summary>
internal sealed class RouteRegistrationWorkflowStatusServiceAssetReportingTarget : IRouteRegistrationWorkflowStatusWriter
{
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _client;

    public RouteRegistrationWorkflowStatusServiceAssetReportingTarget(ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient client) => _client = client;

    public async Task DeliverAsync(RouteRegistrationWorkflowStatusRecord record, CancellationToken token)
    {
        await _client.FetchServiceAssetReportAsync(new ApiItsm.FetchServiceAssetReportRequest { ServiceAssetId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.licensing.v1.LicenseAssignmentApprovalService.</summary>
internal sealed class RouteRegistrationWorkflowStatusLicenseAssignmentApprovalWriter : IRouteRegistrationWorkflowStatusWriter
{
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _client;

    public RouteRegistrationWorkflowStatusLicenseAssignmentApprovalWriter(ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(RouteRegistrationWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.ApproveLicenseAssignmentRequest { LicenseAssignment = new ApiLicensing.LicenseAssignment { LicenseAssignmentId = record.CorrelationId }, Details = new ApiLicensing.ApproveLicenseAssignmentRequest.Types.Details { ApproverEmail = record.Status, Comment = record.Message } }, token, _client.ApproveLicenseAssignmentAsync);
    }
}

/// <summary>Reads acme.corporateevents.v1.RegistrationWorkflowService/PauseRegistrationWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class RouteRegistrationWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient(channel);
        var reply = await source.PauseRegistrationWorkflowAsync(new ApiCorporateevents.PauseRegistrationWorkflowRequest() { RegistrationId = "default-route-registration-workflow-status" }, cancellationToken: token);
        var record = new RouteRegistrationWorkflowStatusRecord
        {
            Status = reply.Outcome.Status,
            Message = reply.Outcome.Message,
            CorrelationId = reply.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IRouteRegistrationWorkflowStatusWriter>
        {
            ["availability-window-approval"] = new RouteRegistrationWorkflowStatusAvailabilityWindowApprovalTarget(new ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient(channel)),
            ["asset-reporting"] = new RouteRegistrationWorkflowStatusServiceAssetReportingTarget(new ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient(channel)),
            ["license-assignment-approval"] = new RouteRegistrationWorkflowStatusLicenseAssignmentApprovalWriter(new ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "license-assignment-approval")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
