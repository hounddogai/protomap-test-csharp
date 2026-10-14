using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiExperiments = Acme.Experiments.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where PushInterviewsId hands each record.</summary>
internal interface IPushInterviewsIdDestination
{
    Task ForwardAsync(PushInterviewsIdRecord record, CancellationToken token);
}

/// <summary>What PushInterviewsId keeps of each PullInterviews response.</summary>
internal sealed class PushInterviewsIdRecord
{
    public string InterviewId { get; init; } = "";
    public string Title { get; init; } = "";
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
}

/// <summary>Hands records to acme.experiments.v1.ExperimentCalculationService.</summary>
internal sealed class PushInterviewsIdExperimentCalculationSink(ApiExperiments.ExperimentCalculationService.ExperimentCalculationServiceClient client) : IPushInterviewsIdDestination
{
    public async Task ForwardAsync(PushInterviewsIdRecord record, CancellationToken token)
    {
        await client.SimulateExperimentAsync(new ApiExperiments.SimulateExperimentRequest { Reason = record.Title, ExperimentId = record.Status, Details = new ApiExperiments.SimulateExperimentRequest.Types.Details { CurrencyCode = record.InterviewId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.itsm.v1.ChangeRequestReportingService.</summary>
internal sealed class PushInterviewsIdChangeRequestReportingTarget(ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient client) : IPushInterviewsIdDestination
{
    public async Task ForwardAsync(PushInterviewsIdRecord record, CancellationToken token)
    {
        await client.CountChangeRequestTotalsAsync(new ApiItsm.CountChangeRequestTotalsRequest { Filter = new ApiItsm.CountChangeRequestTotalsRequest.Types.Filter { OwnerId = record.OutcomeStatus } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.recruiting.v1.InterviewSyncService/PullInterviews and hands what it keeps to the sink of its kind.</summary>
internal static class PushInterviewsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient(channel);
        var found = await source.PullInterviewsAsync(new ApiRecruiting.PullInterviewsRequest() { Reason = "live-push-interviews-id" }, cancellationToken: token);
        var record = new PushInterviewsIdRecord
        {
            InterviewId = found.Interview.InterviewId,
            Title = Text.Normalize(found.Interview.Title),
            Status = found.Result.Status,
            OutcomeStatus = found.Outcome.Status,
        };
        var sinks = new Dictionary<string, IPushInterviewsIdDestination>
        {
            ["experiment-calculation"] = new PushInterviewsIdExperimentCalculationSink(new ApiExperiments.ExperimentCalculationService.ExperimentCalculationServiceClient(channel)),
            ["change-request-reporting"] = new PushInterviewsIdChangeRequestReportingTarget(new ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient(channel)),
        };
        await RoutePushInterviewsIdAsync("change-request-reporting", record, sinks, token);
    }

    private static async Task RoutePushInterviewsIdAsync(string kind, PushInterviewsIdRecord record, IReadOnlyDictionary<string, IPushInterviewsIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
