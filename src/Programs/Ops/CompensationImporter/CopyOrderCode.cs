using Grpc.Net.Client;
using ApiCareplans = Acme.Careplans.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOrders = Acme.Orders.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where CopyOrderCode hands each record.</summary>
internal interface ICopyOrderCodeDestination
{
    Task WriteAsync(CopyOrderCodeRecord record, CancellationToken token);
}

/// <summary>What CopyOrderCode keeps of each CalculateOrder response.</summary>
internal sealed class CopyOrderCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.careplans.v1.CarePlanCalculationService.</summary>
internal sealed class CopyOrderCodeCarePlanCalculationTarget(ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient client) : ICopyOrderCodeDestination
{
    public async Task WriteAsync(CopyOrderCodeRecord record, CancellationToken token)
    {
        await client.CalculateCarePlanAsync(new ApiCareplans.CalculateCarePlanRequest { CarePlan = new ApiCareplans.CarePlan { Notes = record.CorrelationId }, Details = new ApiCareplans.CalculateCarePlanRequest.Types.Details { CurrencyCode = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.itsm.v1.ChangeRequestLifecycleService.</summary>
internal sealed class CopyOrderCodeChangeRequestLifecycleWriter(ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient client) : ICopyOrderCodeDestination
{
    public async Task WriteAsync(CopyOrderCodeRecord record, CancellationToken token)
    {
        await client.ExpireChangeRequestAsync(new ApiItsm.ExpireChangeRequestRequest { Details = new ApiItsm.ExpireChangeRequestRequest.Types.Details { ReasonCode = record.CorrelationId }, Reason = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningWorkflowService.</summary>
internal sealed class CopyOrderCodeJobOpeningWorkflowRelay : ICopyOrderCodeDestination
{
    private readonly ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient _client;

    public CopyOrderCodeJobOpeningWorkflowRelay(ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(CopyOrderCodeRecord record, CancellationToken token)
    {
        await _client.SignalJobOpeningWorkflowAsync(new ApiRecruiting.SignalJobOpeningWorkflowRequest { Details = new ApiRecruiting.SignalJobOpeningWorkflowRequest.Types.Details { InitiatedBy = record.CorrelationId }, JobOpening = new ApiRecruiting.JobOpening { Notes = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.orders.v1.OrderCalculationService/CalculateOrder and hands what it keeps to the sink of its kind.</summary>
internal static class CopyOrderCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOrders.OrderCalculationService.OrderCalculationServiceClient(channel);
        var got = await source.CalculateOrderAsync(new ApiOrders.CalculateOrderRequest() { OrderId = "nightly-copy-order-code" }, cancellationToken: token);
        var record = new CopyOrderCodeRecord
        {
            CurrencyCode = got.Breakdown.CurrencyCode,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ICopyOrderCodeDestination>
        {
            ["care-plan-calculation"] = new CopyOrderCodeCarePlanCalculationTarget(new ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient(channel)),
            ["change-request-lifecycle"] = new CopyOrderCodeChangeRequestLifecycleWriter(new ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient(channel)),
            ["job-opening-workflow"] = new CopyOrderCodeJobOpeningWorkflowRelay(new ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient(channel)),
        };
        await RouteCopyOrderCodeAsync("care-plan-calculation", record, sinks, token);
    }

    private static async Task RouteCopyOrderCodeAsync(string kind, CopyOrderCodeRecord record, IReadOnlyDictionary<string, ICopyOrderCodeDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
