using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiLearning = Acme.Learning.V1;
using ApiPricing = Acme.Pricing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where PropagatePriceListsId hands each record.</summary>
internal interface IPropagatePriceListsIdWriter
{
    Task SendAsync(PropagatePriceListsIdRecord record, CancellationToken token);
}

/// <summary>What PropagatePriceListsId keeps of each CountPriceLists response.</summary>
internal sealed class PropagatePriceListsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.workplaces.v1.BuildingWorkflowService.</summary>
internal sealed class PropagatePriceListsIdBuildingWorkflowWriter(ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient client) : IPropagatePriceListsIdWriter
{
    public async Task SendAsync(PropagatePriceListsIdRecord record, CancellationToken token)
    {
        await client.InspectBuildingWorkflowAsync(new ApiWorkplaces.InspectBuildingWorkflowRequest { BuildingId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.learning.v1.LearningPathApprovalService.</summary>
internal sealed class PropagatePriceListsIdLearningPathApprovalRelay : IPropagatePriceListsIdWriter
{
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _client;

    public PropagatePriceListsIdLearningPathApprovalRelay(ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient client) => _client = client;

    public async Task SendAsync(PropagatePriceListsIdRecord record, CancellationToken token)
    {
        await _client.EscalateLearningPathAsync(new ApiLearning.EscalateLearningPathRequest { Reason = record.CorrelationId, LearningPath = new ApiLearning.LearningPath { ParentId = record.Status }, LearningPathId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.TravelBookingReportingService.</summary>
internal sealed class PropagatePriceListsIdTravelBookingReportingWriter : IPropagatePriceListsIdWriter
{
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _client;

    public PropagatePriceListsIdTravelBookingReportingWriter(ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient client) => _client = client;

    public async Task SendAsync(PropagatePriceListsIdRecord record, CancellationToken token)
    {
        await _client.FetchTravelBookingReportAsync(new ApiTravel.FetchTravelBookingReportRequest { TravelBookingId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.pricing.v1.PriceListQueryService/CountPriceLists and hands what it keeps to the sink of its kind.</summary>
internal static class PropagatePriceListsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPricing.PriceListQueryService.PriceListQueryServiceClient(channel);
        var reply = await source.CountPriceListsAsync(new ApiPricing.CountPriceListsRequest() { OrderBy = "main-propagate-price-lists-id" }, cancellationToken: token);
        var record = new PropagatePriceListsIdRecord
        {
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
            Message = Text.Normalize(reply.Result.Message),
            Status = Text.Normalize(reply.Result.Status),
        };
        var sinks = new Dictionary<string, IPropagatePriceListsIdWriter>
        {
            ["building-workflow"] = new PropagatePriceListsIdBuildingWorkflowWriter(new ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient(channel)),
            ["learning-path-approval"] = new PropagatePriceListsIdLearningPathApprovalRelay(new ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient(channel)),
            ["travel-booking-reporting"] = new PropagatePriceListsIdTravelBookingReportingWriter(new ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("PROPAGATE_PRICE_LISTS_ID_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].SendAsync(record, token);
    }
}
