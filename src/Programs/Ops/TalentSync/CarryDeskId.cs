using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiConfiguration = Acme.Configuration.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where CarryDeskId hands each record.</summary>
internal interface ICarryDeskIdDestination
{
    Task DeliverAsync(CarryDeskIdRecord record, CancellationToken token);
}

/// <summary>What CarryDeskId keeps of each LookupDesk response.</summary>
internal sealed class CarryDeskIdRecord
{
    public string DeskId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Code { get; init; } = "";
}

/// <summary>Passes records to acme.configuration.v1.ConfigSetService.</summary>
internal sealed class CarryDeskIdConfigSetRelay : ICarryDeskIdDestination
{
    private readonly ApiConfiguration.ConfigSetService.ConfigSetServiceClient _client;

    public CarryDeskIdConfigSetRelay(ApiConfiguration.ConfigSetService.ConfigSetServiceClient client) => _client = client;

    public async Task DeliverAsync(CarryDeskIdRecord record, CancellationToken token)
    {
        await _client.CreateConfigSetAsync(new ApiConfiguration.CreateConfigSetRequest { ConfigSet = new ApiConfiguration.ConfigSet { Name = record.Code, ParentId = record.Status }, RequestId = record.DeskId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.itsm.v1.ChangeRequestValidationService.</summary>
internal sealed class CarryDeskIdChangeRequestValidationTarget(ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient client) : ICarryDeskIdDestination
{
    public async Task DeliverAsync(CarryDeskIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.NormalizeChangeRequestRequest { ChangeRequest = new ApiItsm.ChangeRequest { ChangeRequestId = record.DeskId } }, token, client.NormalizeChangeRequestAsync);
    }
}

/// <summary>Reads acme.workplaces.v1.DeskQueryService/LookupDesk and hands what it keeps to the sink of its kind.</summary>
internal static class CarryDeskId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.DeskQueryService.DeskQueryServiceClient(channel);
        var got = await source.LookupDeskAsync(new ApiWorkplaces.LookupDeskRequest() { DeskId = "current-carry-desk-id" }, cancellationToken: token);
        var record = new CarryDeskIdRecord
        {
            DeskId = got.Desk.DeskId,
            Status = got.Result.Status,
            Code = got.Desk.Code,
        };
        var sinks = new Dictionary<string, ICarryDeskIdDestination>
        {
            ["config-set"] = new CarryDeskIdConfigSetRelay(new ApiConfiguration.ConfigSetService.ConfigSetServiceClient(channel)),
            ["change-request-validation"] = new CarryDeskIdChangeRequestValidationTarget(new ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient(channel)),
        };
        await RouteCarryDeskIdAsync("change-request-validation", record, sinks, token);
    }

    private static async Task RouteCarryDeskIdAsync(string kind, CarryDeskIdRecord record, IReadOnlyDictionary<string, ICarryDeskIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
