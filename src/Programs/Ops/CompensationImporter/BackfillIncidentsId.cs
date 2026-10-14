using Grpc.Net.Client;
using ApiFleet = Acme.Fleet.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where BackfillIncidentsId hands each record.</summary>
internal interface IBackfillIncidentsIdSink
{
    Task WriteAsync(BackfillIncidentsIdRecord record, CancellationToken token);
}

/// <summary>What BackfillIncidentsId keeps of each ResolveIncidents response.</summary>
internal sealed class BackfillIncidentsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.fleet.v1.VehicleQueryService.</summary>
internal sealed class BackfillIncidentsIdVehicleQueryWriter : IBackfillIncidentsIdSink
{
    private readonly ApiFleet.VehicleQueryService.VehicleQueryServiceClient _client;

    public BackfillIncidentsIdVehicleQueryWriter(ApiFleet.VehicleQueryService.VehicleQueryServiceClient client) => _client = client;

    public async Task WriteAsync(BackfillIncidentsIdRecord record, CancellationToken token)
    {
        await _client.SummarizeVehiclesAsync(new ApiFleet.SummarizeVehiclesRequest { Details = new ApiFleet.SummarizeVehiclesRequest.Types.Details { Cron = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.scheduling.v1.RosterCalculationService.</summary>
internal sealed class BackfillIncidentsIdRosterCalculationSink : IBackfillIncidentsIdSink
{
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _client;

    public BackfillIncidentsIdRosterCalculationSink(ApiScheduling.RosterCalculationService.RosterCalculationServiceClient client) => _client = client;

    public async Task WriteAsync(BackfillIncidentsIdRecord record, CancellationToken token)
    {
        await _client.SimulateRosterAsync(new ApiScheduling.SimulateRosterRequest { Roster = new ApiScheduling.Roster { RosterId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.IncidentQueryService/ResolveIncidents and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillIncidentsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.IncidentQueryService.IncidentQueryServiceClient(channel);
        var reply = await source.ResolveIncidentsAsync(new ApiItsm.ResolveIncidentsRequest(), cancellationToken: token);
        var record = new BackfillIncidentsIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
        };
        var sinks = new List<IBackfillIncidentsIdSink>
        {
            new BackfillIncidentsIdVehicleQueryWriter(new ApiFleet.VehicleQueryService.VehicleQueryServiceClient(channel)),
            new BackfillIncidentsIdRosterCalculationSink(new ApiScheduling.RosterCalculationService.RosterCalculationServiceClient(channel)),
        };
        await sinks[1].WriteAsync(record, token);
    }
}
