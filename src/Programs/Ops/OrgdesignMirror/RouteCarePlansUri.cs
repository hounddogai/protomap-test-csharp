using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiLearning = Acme.Learning.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where RouteCarePlansUri hands each record.</summary>
internal interface IRouteCarePlansUriWriter
{
    Task DeliverAsync(RouteCarePlansUriRecord record, CancellationToken token);
}

/// <summary>What RouteCarePlansUri keeps of each SummarizeCarePlans response.</summary>
internal sealed class RouteCarePlansUriRecord
{
    public string DownloadUri { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReportId { get; init; } = "";
}

/// <summary>Writes records to acme.workplaces.v1.BuildingQueryService.</summary>
internal sealed class RouteCarePlansUriBuildingQueryTarget(ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient client) : IRouteCarePlansUriWriter
{
    public async Task DeliverAsync(RouteCarePlansUriRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.FindBuildingsRequest { Filter = new ApiWorkplaces.FindBuildingsRequest.Types.Filter { Region = record.Status } }, token, client.FindBuildingsAsync);
    }
}

/// <summary>Sends records to acme.learning.v1.LearningPathCalculationService.</summary>
internal sealed class RouteCarePlansUriLearningPathCalculationRelay(ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient client) : IRouteCarePlansUriWriter
{
    public async Task DeliverAsync(RouteCarePlansUriRecord record, CancellationToken token)
    {
        await client.ExplainLearningPathCalculationAsync(new ApiLearning.ExplainLearningPathCalculationRequest { LearningPath = new ApiLearning.LearningPath { Name = record.ReportId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.benefits.v1.DependentApprovalService.</summary>
internal sealed class RouteCarePlansUriDependentApprovalWriter : IRouteCarePlansUriWriter
{
    private readonly ApiBenefits.DependentApprovalService.DependentApprovalServiceClient _client;

    public RouteCarePlansUriDependentApprovalWriter(ApiBenefits.DependentApprovalService.DependentApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(RouteCarePlansUriRecord record, CancellationToken token)
    {
        await _client.RecallDependentAsync(new ApiBenefits.RecallDependentRequest { DependentId = record.ReportId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteCarePlansUriLogOnly : IRouteCarePlansUriWriter
{
    public Task DeliverAsync(RouteCarePlansUriRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.DownloadUri}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.careplans.v1.CarePlanQueryService/SummarizeCarePlans and hands what it keeps to the sink of its kind.</summary>
internal static class RouteCarePlansUri
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient(channel);
        var response = await source.SummarizeCarePlansAsync(new ApiCareplans.SummarizeCarePlansRequest() { CarePlanId = "primary-route-care-plans-uri" }, cancellationToken: token);
        var record = new RouteCarePlansUriRecord
        {
            DownloadUri = response.DownloadUri,
            Status = Text.Normalize(response.Result.Status),
            ReportId = Text.Normalize(response.ReportId),
        };
        await DispatchRouteCarePlansUriAsync("dependent-approval", record, channel, token);
    }

    private static async Task DispatchRouteCarePlansUriAsync(string kind, RouteCarePlansUriRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<RouteCarePlansUriRecord, CancellationToken, Task> write = new RouteCarePlansUriLogOnly().DeliverAsync;
        switch (kind)
        {
            case "building-query":
                write = new RouteCarePlansUriBuildingQueryTarget(new ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient(channel)).DeliverAsync;
                break;
            case "learning-path-calculation":
                write = new RouteCarePlansUriLearningPathCalculationRelay(new ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient(channel)).DeliverAsync;
                break;
            case "dependent-approval":
                write = new RouteCarePlansUriDependentApprovalWriter(new ApiBenefits.DependentApprovalService.DependentApprovalServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
