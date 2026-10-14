using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiDevices = Acme.Devices.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where CopyDevicePolicyExpiryId hands each record.</summary>
internal interface ICopyDevicePolicyExpiryIdDestination
{
    Task DeliverAsync(CopyDevicePolicyExpiryIdRecord record, CancellationToken token);
}

/// <summary>What CopyDevicePolicyExpiryId keeps of each ScheduleDevicePolicyExpiry response.</summary>
internal sealed class CopyDevicePolicyExpiryIdRecord
{
    public string DevicePolicyId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Hands records to acme.benefits.v1.LifeEventValidationService.</summary>
internal sealed class CopyDevicePolicyExpiryIdLifeEventValidationTarget(ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient client) : ICopyDevicePolicyExpiryIdDestination
{
    public async Task DeliverAsync(CopyDevicePolicyExpiryIdRecord record, CancellationToken token)
    {
        await client.CheckLifeEventEligibilityAsync(new ApiBenefits.CheckLifeEventEligibilityRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.performance.v1.GoalWorkflowService.</summary>
internal sealed class CopyDevicePolicyExpiryIdGoalWorkflowSink : ICopyDevicePolicyExpiryIdDestination
{
    private readonly ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient _client;

    public CopyDevicePolicyExpiryIdGoalWorkflowSink(ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(CopyDevicePolicyExpiryIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.CancelGoalWorkflowRequest { Goal = new ApiPerformance.Goal { Subject = record.Status }, GoalId = record.DevicePolicyId }, token, _client.CancelGoalWorkflowAsync);
    }
}

/// <summary>Writes records to acme.performance.v1.ReviewWorkflowService.</summary>
internal sealed class CopyDevicePolicyExpiryIdReviewWorkflowRelay : ICopyDevicePolicyExpiryIdDestination
{
    private readonly ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient _client;

    public CopyDevicePolicyExpiryIdReviewWorkflowRelay(ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(CopyDevicePolicyExpiryIdRecord record, CancellationToken token)
    {
        await _client.ListReviewWorkflowsAsync(new ApiPerformance.ListReviewWorkflowsRequest { Filter = new ApiPerformance.ListReviewWorkflowsRequest.Types.Filter { Query = record.Status, Region = record.DevicePolicyId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.licensing.v1.LicenseAssignmentApprovalService.</summary>
internal sealed class CopyDevicePolicyExpiryIdLicenseAssignmentApprovalRelay : ICopyDevicePolicyExpiryIdDestination
{
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _client;

    public CopyDevicePolicyExpiryIdLicenseAssignmentApprovalRelay(ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(CopyDevicePolicyExpiryIdRecord record, CancellationToken token)
    {
        await _client.EscalateLicenseAssignmentAsync(new ApiLicensing.EscalateLicenseAssignmentRequest { Reason = record.DevicePolicyId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.devices.v1.DevicePolicyLifecycleService/ScheduleDevicePolicyExpiry and hands what it keeps to the sink of its kind.</summary>
internal static class CopyDevicePolicyExpiryId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient(channel);
        var found = await source.ScheduleDevicePolicyExpiryAsync(new ApiDevices.ScheduleDevicePolicyExpiryRequest() { DevicePolicyId = "live-copy-device-policy-expiry-id" }, cancellationToken: token);
        var record = new CopyDevicePolicyExpiryIdRecord
        {
            DevicePolicyId = found.DevicePolicy.DevicePolicyId,
            Status = found.Outcome.Status,
            Name = found.DevicePolicy.Name,
        };
        var sinks = new Dictionary<string, ICopyDevicePolicyExpiryIdDestination>
        {
            ["life-event-validation"] = new CopyDevicePolicyExpiryIdLifeEventValidationTarget(new ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient(channel)),
            ["goal-workflow"] = new CopyDevicePolicyExpiryIdGoalWorkflowSink(new ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient(channel)),
            ["review-workflow"] = new CopyDevicePolicyExpiryIdReviewWorkflowRelay(new ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient(channel)),
            ["license-assignment-approval"] = new CopyDevicePolicyExpiryIdLicenseAssignmentApprovalRelay(new ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "license-assignment-approval")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
