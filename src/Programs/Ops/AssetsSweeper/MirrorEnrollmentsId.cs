using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where MirrorEnrollmentsId hands each record.</summary>
internal interface IMirrorEnrollmentsIdSink
{
    Task WriteAsync(MirrorEnrollmentsIdRecord record, CancellationToken token);
}

/// <summary>What MirrorEnrollmentsId keeps of each SummarizeEnrollments response.</summary>
internal sealed class MirrorEnrollmentsIdRecord
{
    public string ReportId { get; init; } = "";
    public string Status { get; init; } = "";
    public string DownloadUri { get; init; } = "";
}

/// <summary>Passes records to acme.employeeportal.v1.SelfServiceRequestIntegrationService.</summary>
internal sealed class MirrorEnrollmentsIdSelfServiceRequestIntegrationRelay(ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient client) : IMirrorEnrollmentsIdSink
{
    public async Task WriteAsync(MirrorEnrollmentsIdRecord record, CancellationToken token)
    {
        await client.DisconnectSelfServiceRequestSourceAsync(new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest { Details = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest.Types.Details { Endpoint = record.Status }, Reason = record.ReportId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.CalibrationValidationService.</summary>
internal sealed class MirrorEnrollmentsIdCalibrationValidationWriter : IMirrorEnrollmentsIdSink
{
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _client;

    public MirrorEnrollmentsIdCalibrationValidationWriter(ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient client) => _client = client;

    public async Task WriteAsync(MirrorEnrollmentsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.CheckCalibrationEligibilityRequest { Calibration = new ApiPerformance.Calibration { CalibrationId = record.DownloadUri }, CalibrationId = record.Status, Reason = record.ReportId }, token, _client.CheckCalibrationEligibilityAsync);
    }
}

/// <summary>Hands records to acme.helpdesk.v1.SlaPolicyLifecycleService.</summary>
internal sealed class MirrorEnrollmentsIdSlaPolicyLifecycleTarget : IMirrorEnrollmentsIdSink
{
    private readonly ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient _client;

    public MirrorEnrollmentsIdSlaPolicyLifecycleTarget(ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient client) => _client = client;

    public async Task WriteAsync(MirrorEnrollmentsIdRecord record, CancellationToken token)
    {
        await _client.ExpireSlaPolicyAsync(new ApiHelpdesk.ExpireSlaPolicyRequest { Details = new ApiHelpdesk.ExpireSlaPolicyRequest.Types.Details { ReasonCode = record.DownloadUri }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class MirrorEnrollmentsIdLogOnly : IMirrorEnrollmentsIdSink
{
    public Task WriteAsync(MirrorEnrollmentsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReportId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.benefits.v1.EnrollmentQueryService/SummarizeEnrollments and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorEnrollmentsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient(channel);
        var found = await source.SummarizeEnrollmentsAsync(new ApiBenefits.SummarizeEnrollmentsRequest() { EnrollmentId = "primary-mirror-enrollments-id" }, cancellationToken: token);
        var record = new MirrorEnrollmentsIdRecord
        {
            ReportId = found.ReportId,
            Status = found.Result.Status,
            DownloadUri = found.DownloadUri,
        };
        await DeliverMirrorEnrollmentsIdAsync("calibration-validation", record, channel, token);
    }

    private static async Task DeliverMirrorEnrollmentsIdAsync(string kind, MirrorEnrollmentsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IMirrorEnrollmentsIdSink sink = new MirrorEnrollmentsIdLogOnly();
        switch (kind)
        {
            case "self-request-integration":
                sink = new MirrorEnrollmentsIdSelfServiceRequestIntegrationRelay(new ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient(channel));
                break;
            case "calibration-validation":
                sink = new MirrorEnrollmentsIdCalibrationValidationWriter(new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(channel));
                break;
            case "sla-policy-lifecycle":
                sink = new MirrorEnrollmentsIdSlaPolicyLifecycleTarget(new ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient(channel));
                break;
        }
        await sink.WriteAsync(record, token);
    }
}
