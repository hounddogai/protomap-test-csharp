using Grpc.Net.Client;
using ApiDevices = Acme.Devices.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where RelayCalibrationStatus hands each record.</summary>
internal interface IRelayCalibrationStatusTarget
{
    Task WriteAsync(RelayCalibrationStatusRecord record, CancellationToken token);
}

/// <summary>What RelayCalibrationStatus keeps of each SuspendCalibration response.</summary>
internal sealed class RelayCalibrationStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.licensing.v1.SoftwareLicenseCalculationService.</summary>
internal sealed class RelayCalibrationStatusSoftwareLicenseCalculationWriter(ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient client) : IRelayCalibrationStatusTarget
{
    public async Task WriteAsync(RelayCalibrationStatusRecord record, CancellationToken token)
    {
        await client.CompareSoftwareLicenseOptionsAsync(new ApiLicensing.CompareSoftwareLicenseOptionsRequest { SoftwareLicense = new ApiLicensing.SoftwareLicense { CurrencyCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.surveys.v1.SurveyService.</summary>
internal sealed class RelayCalibrationStatusSurveyRelay(ApiSurveys.SurveyService.SurveyServiceClient client) : IRelayCalibrationStatusTarget
{
    public async Task WriteAsync(RelayCalibrationStatusRecord record, CancellationToken token)
    {
        await client.CloneSurveyAsync(new ApiSurveys.CloneSurveyRequest { Survey = new ApiSurveys.Survey { Notes = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.devices.v1.ManagedDeviceApprovalService.</summary>
internal sealed class RelayCalibrationStatusManagedDeviceApprovalRelay(ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient client) : IRelayCalibrationStatusTarget
{
    public async Task WriteAsync(RelayCalibrationStatusRecord record, CancellationToken token)
    {
        await client.GetManagedDeviceApprovalStatusAsync(new ApiDevices.GetManagedDeviceApprovalStatusRequest { ManagedDeviceId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.performance.v1.CalibrationLifecycleService/SuspendCalibration and hands what it keeps to the sink of its kind.</summary>
internal static class RelayCalibrationStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient(channel);
        var response = await source.SuspendCalibrationAsync(new ApiPerformance.SuspendCalibrationRequest() { CalibrationId = "current-relay-calibration-status" }, cancellationToken: token);
        var record = new RelayCalibrationStatusRecord
        {
            Status = response.Result.Status,
            Message = response.Outcome.Message,
        };
        var sinks = new Dictionary<string, IRelayCalibrationStatusTarget>
        {
            ["software-license-calculation"] = new RelayCalibrationStatusSoftwareLicenseCalculationWriter(new ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient(channel)),
            ["survey"] = new RelayCalibrationStatusSurveyRelay(new ApiSurveys.SurveyService.SurveyServiceClient(channel)),
            ["managed-device-approval"] = new RelayCalibrationStatusManagedDeviceApprovalRelay(new ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient(channel)),
        };
        await RouteRelayCalibrationStatusAsync("survey", record, sinks, token);
    }

    private static async Task RouteRelayCalibrationStatusAsync(string kind, RelayCalibrationStatusRecord record, IReadOnlyDictionary<string, IRelayCalibrationStatusTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
