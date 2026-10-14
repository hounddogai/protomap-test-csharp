using Grpc.Net.Client;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where CarryCorporateEventReportsStatus hands each record.</summary>
internal interface ICarryCorporateEventReportsStatusDestination
{
    Task ForwardAsync(CarryCorporateEventReportsStatusRecord record, CancellationToken token);
}

/// <summary>What CarryCorporateEventReportsStatus keeps of each ListCorporateEventReports response.</summary>
internal sealed class CarryCorporateEventReportsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.recruiting.v1.InterviewWorkflowService.</summary>
internal sealed class CarryCorporateEventReportsStatusInterviewWorkflowWriter : ICarryCorporateEventReportsStatusDestination
{
    private readonly ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient _client;

    public CarryCorporateEventReportsStatusInterviewWorkflowWriter(ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryCorporateEventReportsStatusRecord record, CancellationToken token)
    {
        await _client.SignalInterviewWorkflowAsync(new ApiRecruiting.SignalInterviewWorkflowRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.scheduling.v1.ShiftService.</summary>
internal sealed class CarryCorporateEventReportsStatusShiftTarget : ICarryCorporateEventReportsStatusDestination
{
    private readonly ApiScheduling.ShiftService.ShiftServiceClient _client;

    public CarryCorporateEventReportsStatusShiftTarget(ApiScheduling.ShiftService.ShiftServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryCorporateEventReportsStatusRecord record, CancellationToken token)
    {
        await _client.CreateShiftAsync(new ApiScheduling.CreateShiftRequest { RequestId = record.Status, Shift = new ApiScheduling.Shift { ShiftId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.CalibrationValidationService.</summary>
internal sealed class CarryCorporateEventReportsStatusCalibrationValidationSink : ICarryCorporateEventReportsStatusDestination
{
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _client;

    public CarryCorporateEventReportsStatusCalibrationValidationSink(ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryCorporateEventReportsStatusRecord record, CancellationToken token)
    {
        await _client.ExplainCalibrationDecisionAsync(new ApiPerformance.ExplainCalibrationDecisionRequest { Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventReportingService/ListCorporateEventReports and hands what it keeps to the sink of its kind.</summary>
internal static class CarryCorporateEventReportsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(channel);
        var got = await source.ListCorporateEventReportsAsync(new ApiCorporateevents.ListCorporateEventReportsRequest() { OrderBy = "live-carry-corporate-event-reports-status" }, cancellationToken: token);
        var record = new CarryCorporateEventReportsStatusRecord
        {
            Status = got.Result.Status,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ICarryCorporateEventReportsStatusDestination>
        {
            ["interview-workflow"] = new CarryCorporateEventReportsStatusInterviewWorkflowWriter(new ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient(channel)),
            ["shift"] = new CarryCorporateEventReportsStatusShiftTarget(new ApiScheduling.ShiftService.ShiftServiceClient(channel)),
            ["calibration-validation"] = new CarryCorporateEventReportsStatusCalibrationValidationSink(new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(channel)),
        };
        await RouteCarryCorporateEventReportsStatusAsync("calibration-validation", record, sinks, token);
    }

    private static async Task RouteCarryCorporateEventReportsStatusAsync(string kind, CarryCorporateEventReportsStatusRecord record, IReadOnlyDictionary<string, ICarryCorporateEventReportsStatusDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
