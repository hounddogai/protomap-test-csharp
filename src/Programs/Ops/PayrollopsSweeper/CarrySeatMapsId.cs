using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiReviews = Acme.Reviews.V1;
using ApiSeating = Acme.Seating.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where CarrySeatMapsId hands each record.</summary>
internal interface ICarrySeatMapsIdTarget
{
    Task PushAsync(CarrySeatMapsIdRecord record, CancellationToken token);
}

/// <summary>What CarrySeatMapsId keeps of each SuggestSeatMaps response.</summary>
internal sealed class CarrySeatMapsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.reviews.v1.ProductReviewQueryService.</summary>
internal sealed class CarrySeatMapsIdProductReviewQueryWriter : ICarrySeatMapsIdTarget
{
    private readonly ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient _client;

    public CarrySeatMapsIdProductReviewQueryWriter(ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient client) => _client = client;

    public async Task PushAsync(CarrySeatMapsIdRecord record, CancellationToken token)
    {
        await _client.SuggestProductReviewsAsync(new ApiReviews.SuggestProductReviewsRequest { OrderBy = record.CorrelationId, Filter = new ApiReviews.SuggestProductReviewsRequest.Types.Filter { OwnerId = record.Message, Query = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.pipelines.v1.PipelineService.</summary>
internal sealed class CarrySeatMapsIdPipelineTarget : ICarrySeatMapsIdTarget
{
    private readonly ApiPipelines.PipelineService.PipelineServiceClient _client;

    public CarrySeatMapsIdPipelineTarget(ApiPipelines.PipelineService.PipelineServiceClient client) => _client = client;

    public async Task PushAsync(CarrySeatMapsIdRecord record, CancellationToken token)
    {
        await _client.ListPipelinesAsync(new ApiPipelines.ListPipelinesRequest { OrderBy = record.Status, Filter = new ApiPipelines.ListPipelinesRequest.Types.Filter { Region = record.CorrelationId }, Page = new ApiCommon.PageRequest { PageToken = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CarrySeatMapsIdLogOnly : ICarrySeatMapsIdTarget
{
    public Task PushAsync(CarrySeatMapsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.seating.v1.SeatMapQueryService/SuggestSeatMaps and hands what it keeps to the sink of its kind.</summary>
internal static class CarrySeatMapsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient(channel);
        var response = await source.SuggestSeatMapsAsync(new ApiSeating.SuggestSeatMapsRequest() { OrderBy = "default-carry-seat-maps-id" }, cancellationToken: token);
        var record = new CarrySeatMapsIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Status = Text.Normalize(response.Result.Status),
            Message = response.Result.Message,
        };
        await DeliverCarrySeatMapsIdAsync("product-review-query", record, channel, token);
    }

    private static async Task DeliverCarrySeatMapsIdAsync(string kind, CarrySeatMapsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ICarrySeatMapsIdTarget sink = new CarrySeatMapsIdLogOnly();
        switch (kind)
        {
            case "product-review-query":
                sink = new CarrySeatMapsIdProductReviewQueryWriter(new ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient(channel));
                break;
            case "pipeline":
                sink = new CarrySeatMapsIdPipelineTarget(new ApiPipelines.PipelineService.PipelineServiceClient(channel));
                break;
        }
        await sink.PushAsync(record, token);
    }
}
