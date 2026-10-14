using Grpc.Net.Client;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiContent = Acme.Content.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where RouteArticlesId hands each record.</summary>
internal interface IRouteArticlesIdTarget
{
    Task DeliverAsync(RouteArticlesIdRecord record, CancellationToken token);
}

/// <summary>What RouteArticlesId keeps of each ResolveArticles response.</summary>
internal sealed class RouteArticlesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.ShiftWorkflowService.</summary>
internal sealed class RouteArticlesIdShiftWorkflowWriter : IRouteArticlesIdTarget
{
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _client;

    public RouteArticlesIdShiftWorkflowWriter(ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(RouteArticlesIdRecord record, CancellationToken token)
    {
        await _client.SignalShiftWorkflowAsync(new ApiScheduling.SignalShiftWorkflowRequest { Details = new ApiScheduling.SignalShiftWorkflowRequest.Types.Details { InitiatedBy = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.authorization.v1.AccessPolicyQueryService.</summary>
internal sealed class RouteArticlesIdAccessPolicyQuerySink(ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient client) : IRouteArticlesIdTarget
{
    public async Task DeliverAsync(RouteArticlesIdRecord record, CancellationToken token)
    {
        await client.FindAccessPoliciesAsync(new ApiAuthorization.FindAccessPoliciesRequest { Filter = new ApiAuthorization.FindAccessPoliciesRequest.Types.Filter { Query = record.NextPageToken, Region = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.performance.v1.CalibrationValidationService.</summary>
internal sealed class RouteArticlesIdCalibrationValidationSink(ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient client) : IRouteArticlesIdTarget
{
    public async Task DeliverAsync(RouteArticlesIdRecord record, CancellationToken token)
    {
        await client.LintCalibrationsAsync(new ApiPerformance.LintCalibrationsRequest { CalibrationId = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteArticlesIdLogOnly : IRouteArticlesIdTarget
{
    public Task DeliverAsync(RouteArticlesIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.content.v1.ArticleQueryService/ResolveArticles and hands what it keeps to the sink of its kind.</summary>
internal static class RouteArticlesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiContent.ArticleQueryService.ArticleQueryServiceClient(channel);
        var reply = await source.ResolveArticlesAsync(new ApiContent.ResolveArticlesRequest(), cancellationToken: token);
        var record = new RouteArticlesIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            NextPageToken = reply.PageInfo.NextPageToken,
            Status = reply.Result.Status,
        };
        await DispatchRouteArticlesIdAsync("access-policy-query", record, channel, token);
    }

    private static async Task DispatchRouteArticlesIdAsync(string kind, RouteArticlesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<RouteArticlesIdRecord, CancellationToken, Task> write = new RouteArticlesIdLogOnly().DeliverAsync;
        switch (kind)
        {
            case "shift-workflow":
                write = new RouteArticlesIdShiftWorkflowWriter(new ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient(channel)).DeliverAsync;
                break;
            case "access-policy-query":
                write = new RouteArticlesIdAccessPolicyQuerySink(new ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient(channel)).DeliverAsync;
                break;
            case "calibration-validation":
                write = new RouteArticlesIdCalibrationValidationSink(new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
