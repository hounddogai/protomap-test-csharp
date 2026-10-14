using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiCompliance = Acme.Compliance.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where MirrorCorporateEventWorkflowId hands each record.</summary>
internal interface IMirrorCorporateEventWorkflowIdTarget
{
    Task WriteAsync(MirrorCorporateEventWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What MirrorCorporateEventWorkflowId keeps of each RetryCorporateEventWorkflow response.</summary>
internal sealed class MirrorCorporateEventWorkflowIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.corporateevents.v1.RegistrationCalculationService.</summary>
internal sealed class MirrorCorporateEventWorkflowIdRegistrationCalculationWriter(ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient client) : IMirrorCorporateEventWorkflowIdTarget
{
    public async Task WriteAsync(MirrorCorporateEventWorkflowIdRecord record, CancellationToken token)
    {
        await client.GetRegistrationRatesAsync(new ApiCorporateevents.GetRegistrationRatesRequest { RegistrationId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.compliance.v1.ComplianceCaseQueryService.</summary>
internal sealed class MirrorCorporateEventWorkflowIdComplianceCaseQuerySink(ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient client) : IMirrorCorporateEventWorkflowIdTarget
{
    public async Task WriteAsync(MirrorCorporateEventWorkflowIdRecord record, CancellationToken token)
    {
        await client.CountComplianceCasesAsync(new ApiCompliance.CountComplianceCasesRequest { Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId }, Filter = new ApiCompliance.CountComplianceCasesRequest.Types.Filter { Query = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.configuration.v1.ConfigSetCalculationService.</summary>
internal sealed class MirrorCorporateEventWorkflowIdConfigSetCalculationTarget : IMirrorCorporateEventWorkflowIdTarget
{
    private readonly ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient _client;

    public MirrorCorporateEventWorkflowIdConfigSetCalculationTarget(ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient client) => _client = client;

    public async Task WriteAsync(MirrorCorporateEventWorkflowIdRecord record, CancellationToken token)
    {
        await _client.CalculateConfigSetAsync(new ApiConfiguration.CalculateConfigSetRequest { Details = new ApiConfiguration.CalculateConfigSetRequest.Types.Details { CurrencyCode = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class MirrorCorporateEventWorkflowIdLogOnly : IMirrorCorporateEventWorkflowIdTarget
{
    public Task WriteAsync(MirrorCorporateEventWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReferenceId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventWorkflowService/RetryCorporateEventWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorCorporateEventWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient(channel);
        var response = await source.RetryCorporateEventWorkflowAsync(new ApiCorporateevents.RetryCorporateEventWorkflowRequest() { Reason = "nightly-mirror-corporate-event-workflow-id" }, cancellationToken: token);
        var record = new MirrorCorporateEventWorkflowIdRecord
        {
            ReferenceId = response.Outcome.ReferenceId,
            Message = Text.Normalize(response.Result.Message),
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
        };
        await DispatchMirrorCorporateEventWorkflowIdAsync("compliance-case-query", record, channel, token);
    }

    private static async Task DispatchMirrorCorporateEventWorkflowIdAsync(string kind, MirrorCorporateEventWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<MirrorCorporateEventWorkflowIdRecord, CancellationToken, Task> write = new MirrorCorporateEventWorkflowIdLogOnly().WriteAsync;
        switch (kind)
        {
            case "registration-calculation":
                write = new MirrorCorporateEventWorkflowIdRegistrationCalculationWriter(new ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient(channel)).WriteAsync;
                break;
            case "compliance-case-query":
                write = new MirrorCorporateEventWorkflowIdComplianceCaseQuerySink(new ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient(channel)).WriteAsync;
                break;
            case "config-set-calculation":
                write = new MirrorCorporateEventWorkflowIdConfigSetCalculationTarget(new ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient(channel)).WriteAsync;
                break;
        }
        await write(record, token);
    }
}
