using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiSecrets = Acme.Secrets.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where RelayYardStatus hands each record.</summary>
internal interface IRelayYardStatusTarget
{
    Task WriteAsync(RelayYardStatusRecord record, CancellationToken token);
}

/// <summary>What RelayYardStatus keeps of each QuoteYard response.</summary>
internal sealed class RelayYardStatusRecord
{
    public string Status { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Hands records to acme.corporateevents.v1.VenueReportingService.</summary>
internal sealed class RelayYardStatusVenueReportingSink : IRelayYardStatusTarget
{
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _client;

    public RelayYardStatusVenueReportingSink(ApiCorporateevents.VenueReportingService.VenueReportingServiceClient client) => _client = client;

    public async Task WriteAsync(RelayYardStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.ScheduleVenueReportRequest { VenueId = record.Status, Details = new ApiCorporateevents.ScheduleVenueReportRequest.Types.Details { Cron = record.Name }, Reason = record.Code }, token, _client.ScheduleVenueReportAsync);
    }
}

/// <summary>Hands records to acme.performance.v1.GoalWorkflowService.</summary>
internal sealed class RelayYardStatusGoalWorkflowSink(ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient client) : IRelayYardStatusTarget
{
    public async Task WriteAsync(RelayYardStatusRecord record, CancellationToken token)
    {
        await client.RetryGoalWorkflowAsync(new ApiPerformance.RetryGoalWorkflowRequest { Details = new ApiPerformance.RetryGoalWorkflowRequest.Types.Details { Step = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.fleet.v1.VehicleQueryService.</summary>
internal sealed class RelayYardStatusVehicleQuerySink(ApiFleet.VehicleQueryService.VehicleQueryServiceClient client) : IRelayYardStatusTarget
{
    public async Task WriteAsync(RelayYardStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFleet.SummarizeVehiclesRequest { Details = new ApiFleet.SummarizeVehiclesRequest.Types.Details { Format = record.Name } }, token, client.SummarizeVehiclesAsync);
    }
}

/// <summary>Passes records to acme.secrets.v1.SecretService.</summary>
internal sealed class RelayYardStatusSecretRelay(ApiSecrets.SecretService.SecretServiceClient client) : IRelayYardStatusTarget
{
    public async Task WriteAsync(RelayYardStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiSecrets.GetSecretRequest { SecretId = record.CurrencyCode }, token, client.GetSecretAsync);
    }
}

/// <summary>Reads acme.yards.v1.YardCalculationService/QuoteYard and hands what it keeps to the sink of its kind.</summary>
internal static class RelayYardStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiYards.YardCalculationService.YardCalculationServiceClient(channel);
        var response = await source.QuoteYardAsync(new ApiYards.QuoteYardRequest() { YardId = "live-relay-yard-status" }, cancellationToken: token);
        var record = new RelayYardStatusRecord
        {
            Status = response.Result.Status,
            CurrencyCode = response.Breakdown.CurrencyCode,
            Code = response.Yard.Code,
            Name = response.Yard.Name,
        };
        await ActOnRelayYardStatusAsync("secret", record, channel, token);
    }

    private static async Task ActOnRelayYardStatusAsync(string kind, RelayYardStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["venue-reporting"] = () => new RelayYardStatusVenueReportingSink(new ApiCorporateevents.VenueReportingService.VenueReportingServiceClient(channel)).WriteAsync(record, token),
            ["goal-workflow"] = () => new RelayYardStatusGoalWorkflowSink(new ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient(channel)).WriteAsync(record, token),
            ["vehicle-query"] = () => new RelayYardStatusVehicleQuerySink(new ApiFleet.VehicleQueryService.VehicleQueryServiceClient(channel)).WriteAsync(record, token),
            ["secret"] = () => new RelayYardStatusSecretRelay(new ApiSecrets.SecretService.SecretServiceClient(channel)).WriteAsync(record, token),
        };
        await actions[kind]();
    }
}
