using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDevices = Acme.Devices.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where HandOffSoftwareLicenseMessage hands each record.</summary>
internal interface IHandOffSoftwareLicenseMessageSink
{
    Task SendAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffSoftwareLicenseMessage keeps of each PreviewSoftwareLicense response.</summary>
internal sealed class HandOffSoftwareLicenseMessageRecord
{
    public string Message { get; init; } = "";
    public string SoftwareLicenseId { get; init; } = "";
    public string ResultMessage { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Hands records to acme.devices.v1.ManagedDeviceApprovalService.</summary>
internal sealed class HandOffSoftwareLicenseMessageManagedDeviceApprovalTarget : IHandOffSoftwareLicenseMessageSink
{
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _client;

    public HandOffSoftwareLicenseMessageManagedDeviceApprovalTarget(ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient client) => _client = client;

    public async Task SendAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token)
    {
        await _client.SubmitManagedDeviceAsync(new ApiDevices.SubmitManagedDeviceRequest { Details = new ApiDevices.SubmitManagedDeviceRequest.Types.Details { ApproverEmail = record.ResultMessage } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.devices.v1.ComplianceScanValidationService.</summary>
internal sealed class HandOffSoftwareLicenseMessageComplianceScanValidationSink : IHandOffSoftwareLicenseMessageSink
{
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _client;

    public HandOffSoftwareLicenseMessageComplianceScanValidationSink(ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient client) => _client = client;

    public async Task SendAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token)
    {
        await _client.PreviewComplianceScanAsync(new ApiDevices.PreviewComplianceScanRequest { ComplianceScan = new ApiDevices.ComplianceScan { ComplianceScanId = record.SoftwareLicenseId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.licensing.v1.SoftwareLicenseValidationService/PreviewSoftwareLicense and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffSoftwareLicenseMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient(channel);
        var got = await source.PreviewSoftwareLicenseAsync(new ApiLicensing.PreviewSoftwareLicenseRequest() { Reason = "primary-hand-off-software-license-message" }, cancellationToken: token);
        var record = new HandOffSoftwareLicenseMessageRecord
        {
            Message = got.Outcome.Message,
            SoftwareLicenseId = Text.Normalize(got.SoftwareLicense.SoftwareLicenseId),
            ResultMessage = got.Result.Message,
            CurrencyCode = got.SoftwareLicense.CurrencyCode,
        };
        var sinks = new Dictionary<string, IHandOffSoftwareLicenseMessageSink>
        {
            ["managed-device-approval"] = new HandOffSoftwareLicenseMessageManagedDeviceApprovalTarget(new ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient(channel)),
            ["compliance-scan-validation"] = new HandOffSoftwareLicenseMessageComplianceScanValidationSink(new ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient(channel)),
        };
        await RouteHandOffSoftwareLicenseMessageAsync("compliance-scan-validation", record, sinks, token);
    }

    private static async Task RouteHandOffSoftwareLicenseMessageAsync(string kind, HandOffSoftwareLicenseMessageRecord record, IReadOnlyDictionary<string, IHandOffSoftwareLicenseMessageSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
