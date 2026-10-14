using Grpc.Net.Client;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where PushChangeRequestMessage hands each record.</summary>
internal interface IPushChangeRequestMessageWriter
{
    Task WriteAsync(PushChangeRequestMessageRecord record, CancellationToken token);
}

/// <summary>What PushChangeRequestMessage keeps of each ValidateChangeRequest response.</summary>
internal sealed class PushChangeRequestMessageRecord
{
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Hands records to acme.performance.v1.CalibrationValidationService.</summary>
internal sealed class PushChangeRequestMessageCalibrationValidationSink : IPushChangeRequestMessageWriter
{
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _client;

    public PushChangeRequestMessageCalibrationValidationSink(ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient client) => _client = client;

    public async Task WriteAsync(PushChangeRequestMessageRecord record, CancellationToken token)
    {
        await _client.LintCalibrationsAsync(new ApiPerformance.LintCalibrationsRequest { Details = new ApiPerformance.LintCalibrationsRequest.Types.Details { Locale = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.scheduling.v1.AvailabilityWindowCalculationService.</summary>
internal sealed class PushChangeRequestMessageAvailabilityWindowCalculationWriter : IPushChangeRequestMessageWriter
{
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _client;

    public PushChangeRequestMessageAvailabilityWindowCalculationWriter(ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient client) => _client = client;

    public async Task WriteAsync(PushChangeRequestMessageRecord record, CancellationToken token)
    {
        await _client.CalculateAvailabilityWindowAsync(new ApiScheduling.CalculateAvailabilityWindowRequest { Reason = record.OutcomeMessage, AvailabilityWindow = new ApiScheduling.AvailabilityWindow { Title = record.Message, AvailabilityWindowId = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.licensing.v1.LicenseAssignmentApprovalService.</summary>
internal sealed class PushChangeRequestMessageLicenseAssignmentApprovalWriter : IPushChangeRequestMessageWriter
{
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _client;

    public PushChangeRequestMessageLicenseAssignmentApprovalWriter(ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(PushChangeRequestMessageRecord record, CancellationToken token)
    {
        await _client.ListPendingLicenseAssignmentsAsync(new ApiLicensing.ListPendingLicenseAssignmentsRequest { OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ChangeRequestValidationService/ValidateChangeRequest and hands what it keeps to the sink of its kind.</summary>
internal static class PushChangeRequestMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient(channel);
        var response = await source.ValidateChangeRequestAsync(new ApiItsm.ValidateChangeRequestRequest() { ChangeRequestId = "current-push-change-request-message" }, cancellationToken: token);
        var record = new PushChangeRequestMessageRecord
        {
            Message = response.Result.Message,
            ReferenceId = response.Outcome.ReferenceId,
            OutcomeMessage = response.Outcome.Message,
            Subject = response.ChangeRequest.Subject,
        };
        var sinks = new List<IPushChangeRequestMessageWriter>
        {
            new PushChangeRequestMessageCalibrationValidationSink(new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(channel)),
            new PushChangeRequestMessageAvailabilityWindowCalculationWriter(new ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient(channel)),
            new PushChangeRequestMessageLicenseAssignmentApprovalWriter(new ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.WriteAsync(record, token);
    }
}
