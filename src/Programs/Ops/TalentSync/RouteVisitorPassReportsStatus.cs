using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiLearning = Acme.Learning.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where RouteVisitorPassReportsStatus hands each record.</summary>
internal interface IRouteVisitorPassReportsStatusTarget
{
    Task PushAsync(RouteVisitorPassReportsStatusRecord record, CancellationToken token);
}

/// <summary>What RouteVisitorPassReportsStatus keeps of each ListVisitorPassReports response.</summary>
internal sealed class RouteVisitorPassReportsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.learning.v1.LearningPathCalculationService.</summary>
internal sealed class RouteVisitorPassReportsStatusLearningPathCalculationSink : IRouteVisitorPassReportsStatusTarget
{
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _client;

    public RouteVisitorPassReportsStatusLearningPathCalculationSink(ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient client) => _client = client;

    public async Task PushAsync(RouteVisitorPassReportsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.RecalculateLearningPathsRequest { Reason = record.CorrelationId }, token, _client.RecalculateLearningPathsAsync);
    }
}

/// <summary>Sends records to acme.workplaces.v1.RoomBookingLifecycleService.</summary>
internal sealed class RouteVisitorPassReportsStatusRoomBookingLifecycleRelay(ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient client) : IRouteVisitorPassReportsStatusTarget
{
    public async Task PushAsync(RouteVisitorPassReportsStatusRecord record, CancellationToken token)
    {
        await client.SuspendRoomBookingAsync(new ApiWorkplaces.SuspendRoomBookingRequest { Details = new ApiWorkplaces.SuspendRoomBookingRequest.Types.Details { ReasonCode = record.NextPageToken }, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.VisitorPassReportingService/ListVisitorPassReports and hands what it keeps to the sink of its kind.</summary>
internal static class RouteVisitorPassReportsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient(channel);
        var found = await source.ListVisitorPassReportsAsync(new ApiWorkplaces.ListVisitorPassReportsRequest() { OrderBy = "main-route-visitor-pass-reports-status" }, cancellationToken: token);
        var record = new RouteVisitorPassReportsStatusRecord
        {
            Status = found.Result.Status,
            CorrelationId = found.Result.CorrelationId,
            NextPageToken = Text.Normalize(found.PageInfo.NextPageToken),
            Message = Text.Normalize(found.Result.Message),
        };
        var sinks = new List<IRouteVisitorPassReportsStatusTarget>
        {
            new RouteVisitorPassReportsStatusLearningPathCalculationSink(new ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient(channel)),
            new RouteVisitorPassReportsStatusRoomBookingLifecycleRelay(new ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient(channel)),
        };
        await sinks[1].PushAsync(record, token);
    }
}
