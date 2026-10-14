using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where CopyAnnouncementsStatus hands each record.</summary>
internal interface ICopyAnnouncementsStatusSink
{
    Task DeliverAsync(CopyAnnouncementsStatusRecord record, CancellationToken token);
}

/// <summary>What CopyAnnouncementsStatus keeps of each CountAnnouncements response.</summary>
internal sealed class CopyAnnouncementsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.learning.v1.LearningPathWorkflowService.</summary>
internal sealed class CopyAnnouncementsStatusLearningPathWorkflowTarget : ICopyAnnouncementsStatusSink
{
    private readonly ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient _client;

    public CopyAnnouncementsStatusLearningPathWorkflowTarget(ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(CopyAnnouncementsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.ListLearningPathWorkflowsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Message } }, token, _client.ListLearningPathWorkflowsAsync);
    }
}

/// <summary>Sends records to acme.timekeeping.v1.ClockEventQueryService.</summary>
internal sealed class CopyAnnouncementsStatusClockEventQuerySink : ICopyAnnouncementsStatusSink
{
    private readonly ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient _client;

    public CopyAnnouncementsStatusClockEventQuerySink(ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(CopyAnnouncementsStatusRecord record, CancellationToken token)
    {
        await _client.SuggestClockEventsAsync(new ApiTimekeeping.SuggestClockEventsRequest { OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.InterviewLifecycleService.</summary>
internal sealed class CopyAnnouncementsStatusInterviewLifecycleTarget : ICopyAnnouncementsStatusSink
{
    private readonly ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient _client;

    public CopyAnnouncementsStatusInterviewLifecycleTarget(ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient client) => _client = client;

    public async Task DeliverAsync(CopyAnnouncementsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.RenewInterviewRequest { Details = new ApiRecruiting.RenewInterviewRequest.Types.Details { ReasonCode = record.CorrelationId }, Reason = record.Message, InterviewId = record.Status }, token, _client.RenewInterviewAsync);
    }
}

/// <summary>Writes records to acme.performance.v1.CalibrationCalculationService.</summary>
internal sealed class CopyAnnouncementsStatusCalibrationCalculationTarget(ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient client) : ICopyAnnouncementsStatusSink
{
    public async Task DeliverAsync(CopyAnnouncementsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.RecalculateCalibrationsRequest { Calibration = new ApiPerformance.Calibration { CalibrationId = record.CorrelationId }, CalibrationId = record.Message, Details = new ApiPerformance.RecalculateCalibrationsRequest.Types.Details { CurrencyCode = record.Status } }, token, client.RecalculateCalibrationsAsync);
    }
}

/// <summary>Reads acme.employeeportal.v1.AnnouncementQueryService/CountAnnouncements and hands what it keeps to the sink of its kind.</summary>
internal static class CopyAnnouncementsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployeeportal.AnnouncementQueryService.AnnouncementQueryServiceClient(channel);
        var got = await source.CountAnnouncementsAsync(new ApiEmployeeportal.CountAnnouncementsRequest() { OrderBy = "primary-copy-announcements-status" }, cancellationToken: token);
        var record = new CopyAnnouncementsStatusRecord
        {
            Status = got.Result.Status,
            CorrelationId = Text.Normalize(got.Result.CorrelationId),
            Message = got.Result.Message,
        };
        await ActOnCopyAnnouncementsStatusAsync("calibration-calculation", record, channel, token);
    }

    private static async Task ActOnCopyAnnouncementsStatusAsync(string kind, CopyAnnouncementsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["learning-path-workflow"] = () => new CopyAnnouncementsStatusLearningPathWorkflowTarget(new ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient(channel)).DeliverAsync(record, token),
            ["clock-event-query"] = () => new CopyAnnouncementsStatusClockEventQuerySink(new ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient(channel)).DeliverAsync(record, token),
            ["interview-lifecycle"] = () => new CopyAnnouncementsStatusInterviewLifecycleTarget(new ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient(channel)).DeliverAsync(record, token),
            ["calibration-calculation"] = () => new CopyAnnouncementsStatusCalibrationCalculationTarget(new ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
