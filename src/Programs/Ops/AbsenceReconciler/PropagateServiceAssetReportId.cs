using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiDevices = Acme.Devices.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where PropagateServiceAssetReportId hands each record.</summary>
internal interface IPropagateServiceAssetReportIdWriter
{
    Task ApplyAsync(PropagateServiceAssetReportIdRecord record, CancellationToken token);
}

/// <summary>What PropagateServiceAssetReportId keeps of each ShareServiceAssetReport response.</summary>
internal sealed class PropagateServiceAssetReportIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string ServiceAssetId { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Sends records to acme.devices.v1.DevicePolicyValidationService.</summary>
internal sealed class PropagateServiceAssetReportIdDevicePolicyValidationTarget(ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient client) : IPropagateServiceAssetReportIdWriter
{
    public async Task ApplyAsync(PropagateServiceAssetReportIdRecord record, CancellationToken token)
    {
        await client.NormalizeDevicePolicyAsync(new ApiDevices.NormalizeDevicePolicyRequest { Reason = record.ReferenceId, Details = new ApiDevices.NormalizeDevicePolicyRequest.Types.Details { Locale = record.ServiceAssetId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.featureflags.v1.FeatureFlagValidationService.</summary>
internal sealed class PropagateServiceAssetReportIdFeatureFlagValidationTarget(ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient client) : IPropagateServiceAssetReportIdWriter
{
    public async Task ApplyAsync(PropagateServiceAssetReportIdRecord record, CancellationToken token)
    {
        await client.NormalizeFeatureFlagAsync(new ApiFeatureflags.NormalizeFeatureFlagRequest { Reason = record.CorrelationId, FeatureFlag = new ApiFeatureflags.FeatureFlag { Description = record.ServiceAssetId, Name = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.benefits.v1.EnrollmentWorkflowService.</summary>
internal sealed class PropagateServiceAssetReportIdEnrollmentWorkflowSink(ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient client) : IPropagateServiceAssetReportIdWriter
{
    public async Task ApplyAsync(PropagateServiceAssetReportIdRecord record, CancellationToken token)
    {
        await client.InspectEnrollmentWorkflowAsync(new ApiBenefits.InspectEnrollmentWorkflowRequest { EnrollmentId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ServiceAssetReportingService/ShareServiceAssetReport and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateServiceAssetReportId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient(channel);
        var got = await source.ShareServiceAssetReportAsync(new ApiItsm.ShareServiceAssetReportRequest() { Reason = "current-propagate-service-asset-report-id" }, cancellationToken: token);
        var record = new PropagateServiceAssetReportIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            ServiceAssetId = got.ServiceAsset.ServiceAssetId,
            ReferenceId = Text.Normalize(got.Outcome.ReferenceId),
        };
        var sinks = new Dictionary<string, IPropagateServiceAssetReportIdWriter>
        {
            ["device-policy-validation"] = new PropagateServiceAssetReportIdDevicePolicyValidationTarget(new ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient(channel)),
            ["feature-flag-validation"] = new PropagateServiceAssetReportIdFeatureFlagValidationTarget(new ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient(channel)),
            ["enrollment-workflow"] = new PropagateServiceAssetReportIdEnrollmentWorkflowSink(new ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "device-policy-validation")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
