using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where RouteMetricSeriesId hands each record.</summary>
internal interface IRouteMetricSeriesIdTarget
{
    Task ApplyAsync(RouteMetricSeriesIdRecord record, CancellationToken token);
}

/// <summary>What RouteMetricSeriesId keeps of each NormalizeMetricSeries response.</summary>
internal sealed class RouteMetricSeriesIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.appointments.v1.AppointmentQueryService.</summary>
internal sealed class RouteMetricSeriesIdAppointmentQueryRelay(ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient client) : IRouteMetricSeriesIdTarget
{
    public async Task ApplyAsync(RouteMetricSeriesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAppointments.CountAppointmentsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Code } }, token, client.CountAppointmentsAsync);
    }
}

/// <summary>Writes records to acme.itsm.v1.ProblemSyncService.</summary>
internal sealed class RouteMetricSeriesIdProblemSyncWriter(ApiItsm.ProblemSyncService.ProblemSyncServiceClient client) : IRouteMetricSeriesIdTarget
{
    public async Task ApplyAsync(RouteMetricSeriesIdRecord record, CancellationToken token)
    {
        await client.PushProblemAsync(new ApiItsm.PushProblemRequest { Details = new ApiItsm.PushProblemRequest.Types.Details { Cursor = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.metrics.v1.MetricSeriesValidationService/NormalizeMetricSeries and hands what it keeps to the sink of its kind.</summary>
internal static class RouteMetricSeriesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient(channel);
        var found = await source.NormalizeMetricSeriesAsync(new ApiMetrics.NormalizeMetricSeriesRequest() { MetricSeriesId = "default-route-metric-series-id" }, cancellationToken: token);
        var record = new RouteMetricSeriesIdRecord
        {
            ReferenceId = Text.Normalize(found.Outcome.ReferenceId),
            Code = found.MetricSeries.Code,
            Message = found.Result.Message,
        };
        var sinks = new List<IRouteMetricSeriesIdTarget>
        {
            new RouteMetricSeriesIdAppointmentQueryRelay(new ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient(channel)),
            new RouteMetricSeriesIdProblemSyncWriter(new ApiItsm.ProblemSyncService.ProblemSyncServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.ApplyAsync(record, token);
    }
}
