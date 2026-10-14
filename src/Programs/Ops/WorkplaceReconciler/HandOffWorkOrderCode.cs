using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>What HandOffWorkOrderCode keeps of each EstimateWorkOrder response.</summary>
internal sealed class HandOffWorkOrderCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string WorkOrderId { get; init; } = "";
}

/// <summary>Sends records to acme.timekeeping.v1.ClockEventValidationService.</summary>
internal sealed class HandOffWorkOrderCodeClockEventValidationRelay
{
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _client;

    public HandOffWorkOrderCodeClockEventValidationRelay(ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient client) => _client = client;

    public async Task PushAsync(HandOffWorkOrderCodeRecord record, CancellationToken token)
    {
        await _client.GetClockEventRulesAsync(new ApiTimekeeping.GetClockEventRulesRequest { ClockEventId = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Reads acme.maintenance.v1.WorkOrderCalculationService/EstimateWorkOrder and hands what it keeps to acme.timekeeping.v1.ClockEventValidationService.</summary>
internal static class HandOffWorkOrderCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient(channel);
        var found = await source.EstimateWorkOrderAsync(new ApiMaintenance.EstimateWorkOrderRequest() { Reason = "nightly-hand-off-work-order-code" }, cancellationToken: token);
        var record = new HandOffWorkOrderCodeRecord
        {
            CurrencyCode = found.Breakdown.CurrencyCode,
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            WorkOrderId = found.WorkOrder.WorkOrderId,
        };
        await new HandOffWorkOrderCodeClockEventValidationRelay(new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(channel)).PushAsync(record, token);
    }
}
