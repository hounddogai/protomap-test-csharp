using Grpc.Net.Client;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where BackfillEntitlementCalculationId hands each record.</summary>
internal interface IBackfillEntitlementCalculationIdTarget
{
    Task ApplyAsync(BackfillEntitlementCalculationIdRecord record, CancellationToken token);
}

/// <summary>What BackfillEntitlementCalculationId keeps of each ExplainEntitlementCalculation response.</summary>
internal sealed class BackfillEntitlementCalculationIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Description { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.patients.v1.PatientQueryService.</summary>
internal sealed class BackfillEntitlementCalculationIdPatientQueryTarget(ApiPatients.PatientQueryService.PatientQueryServiceClient client) : IBackfillEntitlementCalculationIdTarget
{
    public async Task ApplyAsync(BackfillEntitlementCalculationIdRecord record, CancellationToken token)
    {
        await client.CountPatientsAsync(new ApiPatients.CountPatientsRequest { Filter = new ApiPatients.CountPatientsRequest.Types.Filter { OwnerId = record.Message, Region = record.Description } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.itsm.v1.ServiceAssetCalculationService.</summary>
internal sealed class BackfillEntitlementCalculationIdServiceAssetCalculationSink : IBackfillEntitlementCalculationIdTarget
{
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _client;

    public BackfillEntitlementCalculationIdServiceAssetCalculationSink(ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillEntitlementCalculationIdRecord record, CancellationToken token)
    {
        await _client.CalculateServiceAssetAsync(new ApiItsm.CalculateServiceAssetRequest { ServiceAsset = new ApiItsm.ServiceAsset { Code = record.Description } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.licensing.v1.EntitlementCalculationService/ExplainEntitlementCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillEntitlementCalculationId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient(channel);
        var reply = await source.ExplainEntitlementCalculationAsync(new ApiLicensing.ExplainEntitlementCalculationRequest() { Reason = "current-backfill-entitlement-calculation-id" }, cancellationToken: token);
        var record = new BackfillEntitlementCalculationIdRecord
        {
            ReferenceId = reply.Outcome.ReferenceId,
            Status = reply.Result.Status,
            Description = reply.Entitlement.Description,
            Message = reply.Result.Message,
        };
        await ActOnBackfillEntitlementCalculationIdAsync("asset-calculation", record, channel, token);
    }

    private static async Task ActOnBackfillEntitlementCalculationIdAsync(string kind, BackfillEntitlementCalculationIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["patient-query"] = () => new BackfillEntitlementCalculationIdPatientQueryTarget(new ApiPatients.PatientQueryService.PatientQueryServiceClient(channel)).ApplyAsync(record, token),
            ["asset-calculation"] = () => new BackfillEntitlementCalculationIdServiceAssetCalculationSink(new ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
