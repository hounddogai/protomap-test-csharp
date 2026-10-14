using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ForwardVisitorPassNumber hands each record.</summary>
internal interface IForwardVisitorPassNumberDestination
{
    Task SendAsync(ForwardVisitorPassNumberRecord record, CancellationToken token);
}

/// <summary>What ForwardVisitorPassNumber keeps of each EstimateVisitorPass response.</summary>
internal sealed class ForwardVisitorPassNumberRecord
{
    public string Number { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Passes records to acme.travel.v1.TripService.</summary>
internal sealed class ForwardVisitorPassNumberTripTarget(ApiTravel.TripService.TripServiceClient client) : IForwardVisitorPassNumberDestination
{
    public async Task SendAsync(ForwardVisitorPassNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.CloneTripRequest { Trip = new ApiTravel.Trip { Notes = record.Notes } }, token, client.CloneTripAsync);
    }
}

/// <summary>Hands records to acme.corporateevents.v1.VenueApprovalService.</summary>
internal sealed class ForwardVisitorPassNumberVenueApprovalTarget(ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient client) : IForwardVisitorPassNumberDestination
{
    public async Task SendAsync(ForwardVisitorPassNumberRecord record, CancellationToken token)
    {
        await client.RejectVenueAsync(new ApiCorporateevents.RejectVenueRequest { Venue = new ApiCorporateevents.Venue { Name = record.Number }, Reason = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.scheduling.v1.RosterQueryService.</summary>
internal sealed class ForwardVisitorPassNumberRosterQueryRelay(ApiScheduling.RosterQueryService.RosterQueryServiceClient client) : IForwardVisitorPassNumberDestination
{
    public async Task SendAsync(ForwardVisitorPassNumberRecord record, CancellationToken token)
    {
        await client.CountRostersAsync(new ApiScheduling.CountRostersRequest { OrderBy = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.helpdesk.v1.SlaPolicyLifecycleService.</summary>
internal sealed class ForwardVisitorPassNumberSlaPolicyLifecycleTarget(ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient client) : IForwardVisitorPassNumberDestination
{
    public async Task SendAsync(ForwardVisitorPassNumberRecord record, CancellationToken token)
    {
        await client.ExpireSlaPolicyAsync(new ApiHelpdesk.ExpireSlaPolicyRequest { Reason = record.Number, SlaPolicyId = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.VisitorPassCalculationService/EstimateVisitorPass and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardVisitorPassNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient(channel);
        var got = await source.EstimateVisitorPassAsync(new ApiWorkplaces.EstimateVisitorPassRequest() { Reason = "current-forward-visitor-pass-number" }, cancellationToken: token);
        var record = new ForwardVisitorPassNumberRecord
        {
            Number = got.VisitorPass.Number,
            Notes = got.VisitorPass.Notes,
        };
        var sinks = new List<IForwardVisitorPassNumberDestination>
        {
            new ForwardVisitorPassNumberTripTarget(new ApiTravel.TripService.TripServiceClient(channel)),
            new ForwardVisitorPassNumberVenueApprovalTarget(new ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient(channel)),
            new ForwardVisitorPassNumberRosterQueryRelay(new ApiScheduling.RosterQueryService.RosterQueryServiceClient(channel)),
            new ForwardVisitorPassNumberSlaPolicyLifecycleTarget(new ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.SendAsync(record, token);
    }
}
