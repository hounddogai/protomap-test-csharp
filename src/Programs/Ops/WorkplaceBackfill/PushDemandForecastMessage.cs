using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where PushDemandForecastMessage hands each record.</summary>
internal interface IPushDemandForecastMessageDestination
{
    Task PushAsync(PushDemandForecastMessageRecord record, CancellationToken token);
}

/// <summary>What PushDemandForecastMessage keeps of each CreateDemandForecast response.</summary>
internal sealed class PushDemandForecastMessageRecord
{
    public string Message { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Hands records to acme.featurestore.v1.FeatureViewCalculationService.</summary>
internal sealed class PushDemandForecastMessageFeatureViewCalculationSink(ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient client) : IPushDemandForecastMessageDestination
{
    public async Task PushAsync(PushDemandForecastMessageRecord record, CancellationToken token)
    {
        await client.QuoteFeatureViewAsync(new ApiFeaturestore.QuoteFeatureViewRequest { FeatureViewId = record.Message, FeatureView = new ApiFeaturestore.FeatureView { FeatureViewId = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.travel.v1.TravelBookingApprovalService.</summary>
internal sealed class PushDemandForecastMessageTravelBookingApprovalTarget(ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient client) : IPushDemandForecastMessageDestination
{
    public async Task PushAsync(PushDemandForecastMessageRecord record, CancellationToken token)
    {
        await client.ApproveTravelBookingAsync(new ApiTravel.ApproveTravelBookingRequest { TravelBooking = new ApiTravel.TravelBooking { Notes = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.forecasting.v1.DemandForecastService/CreateDemandForecast and hands what it keeps to the sink of its kind.</summary>
internal static class PushDemandForecastMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiForecasting.DemandForecastService.DemandForecastServiceClient(channel);
        var got = await source.CreateDemandForecastAsync(new ApiForecasting.CreateDemandForecastRequest() { RequestId = "live-push-demand-forecast-message" }, cancellationToken: token);
        var record = new PushDemandForecastMessageRecord
        {
            Message = got.Result.Message,
            Number = Text.Normalize(got.DemandForecast.Number),
        };
        var sinks = new List<IPushDemandForecastMessageDestination>
        {
            new PushDemandForecastMessageFeatureViewCalculationSink(new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(channel)),
            new PushDemandForecastMessageTravelBookingApprovalTarget(new ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.PushAsync(record, token);
    }
}
