using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCareplans = Acme.Careplans.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where BackfillCarePlanId hands each record.</summary>
internal interface IBackfillCarePlanIdDestination
{
    Task WriteAsync(BackfillCarePlanIdRecord record, CancellationToken token);
}

/// <summary>What BackfillCarePlanId keeps of each QuoteCarePlan response.</summary>
internal sealed class BackfillCarePlanIdRecord
{
    public string CarePlanId { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Passes records to acme.travel.v1.TravelBookingLifecycleService.</summary>
internal sealed class BackfillCarePlanIdTravelBookingLifecycleWriter : IBackfillCarePlanIdDestination
{
    private readonly ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient _client;

    public BackfillCarePlanIdTravelBookingLifecycleWriter(ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient client) => _client = client;

    public async Task WriteAsync(BackfillCarePlanIdRecord record, CancellationToken token)
    {
        await _client.GetTravelBookingLifecycleAsync(new ApiTravel.GetTravelBookingLifecycleRequest { TravelBookingId = record.CarePlanId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.workplaces.v1.DeskQueryService.</summary>
internal sealed class BackfillCarePlanIdDeskQueryRelay(ApiWorkplaces.DeskQueryService.DeskQueryServiceClient client) : IBackfillCarePlanIdDestination
{
    public async Task WriteAsync(BackfillCarePlanIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.PeekDeskRequest { DeskId = record.Number }, token, client.PeekDeskAsync);
    }
}

/// <summary>Reads acme.careplans.v1.CarePlanCalculationService/QuoteCarePlan and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillCarePlanId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient(channel);
        var got = await source.QuoteCarePlanAsync(new ApiCareplans.QuoteCarePlanRequest() { CarePlanId = "main-backfill-care-plan-id" }, cancellationToken: token);
        var record = new BackfillCarePlanIdRecord
        {
            CarePlanId = Text.Normalize(got.CarePlan.CarePlanId),
            Number = got.CarePlan.Number,
        };
        var sinks = new List<IBackfillCarePlanIdDestination>
        {
            new BackfillCarePlanIdTravelBookingLifecycleWriter(new ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient(channel)),
            new BackfillCarePlanIdDeskQueryRelay(new ApiWorkplaces.DeskQueryService.DeskQueryServiceClient(channel)),
        };
        await sinks[1].WriteAsync(record, token);
    }
}
