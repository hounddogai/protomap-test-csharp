using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where PropagateGoalsMessage hands each record.</summary>
internal interface IPropagateGoalsMessageWriter
{
    Task ForwardAsync(PropagateGoalsMessageRecord record, CancellationToken token);
}

/// <summary>What PropagateGoalsMessage keeps of each CountGoals response.</summary>
internal sealed class PropagateGoalsMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.metrics.v1.MetricSeriesValidationService.</summary>
internal sealed class PropagateGoalsMessageMetricSeriesValidationWriter : IPropagateGoalsMessageWriter
{
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _client;

    public PropagateGoalsMessageMetricSeriesValidationWriter(ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(PropagateGoalsMessageRecord record, CancellationToken token)
    {
        await _client.LintMetricSeriesesAsync(new ApiMetrics.LintMetricSeriesesRequest { Details = new ApiMetrics.LintMetricSeriesesRequest.Types.Details { Locale = record.Message }, MetricSeries = new ApiMetrics.MetricSeries { Name = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.itsm.v1.ProblemLifecycleService.</summary>
internal sealed class PropagateGoalsMessageProblemLifecycleWriter(ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient client) : IPropagateGoalsMessageWriter
{
    public async Task ForwardAsync(PropagateGoalsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.SuspendProblemRequest { Details = new ApiItsm.SuspendProblemRequest.Types.Details { ReasonCode = record.Status }, ProblemId = record.Message }, token, client.SuspendProblemAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateGoalsMessageLogOnly : IPropagateGoalsMessageWriter
{
    public Task ForwardAsync(PropagateGoalsMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.performance.v1.GoalQueryService/CountGoals and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateGoalsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.GoalQueryService.GoalQueryServiceClient(channel);
        var reply = await source.CountGoalsAsync(new ApiPerformance.CountGoalsRequest() { OrderBy = "current-propagate-goals-message" }, cancellationToken: token);
        var record = new PropagateGoalsMessageRecord
        {
            Message = reply.Result.Message,
            Status = reply.Result.Status,
        };
        await DispatchPropagateGoalsMessageAsync("problem-lifecycle", record, channel, token);
    }

    private static async Task DispatchPropagateGoalsMessageAsync(string kind, PropagateGoalsMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<PropagateGoalsMessageRecord, CancellationToken, Task> write = new PropagateGoalsMessageLogOnly().ForwardAsync;
        switch (kind)
        {
            case "metric-series-validation":
                write = new PropagateGoalsMessageMetricSeriesValidationWriter(new ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient(channel)).ForwardAsync;
                break;
            case "problem-lifecycle":
                write = new PropagateGoalsMessageProblemLifecycleWriter(new ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient(channel)).ForwardAsync;
                break;
        }
        await write(record, token);
    }
}
