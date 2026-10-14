using Grpc.Net.Client;
using ApiDispatch = Acme.Dispatch.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where PropagateRoomBookingWorkflowId hands each record.</summary>
internal interface IPropagateRoomBookingWorkflowIdTarget
{
    Task ApplyAsync(PropagateRoomBookingWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What PropagateRoomBookingWorkflowId keeps of each ResumeRoomBookingWorkflow response.</summary>
internal sealed class PropagateRoomBookingWorkflowIdRecord
{
    public string RoomBookingId { get; init; } = "";
    public string Title { get; init; } = "";
}

/// <summary>Writes records to acme.timekeeping.v1.ClockEventValidationService.</summary>
internal sealed class PropagateRoomBookingWorkflowIdClockEventValidationSink(ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient client) : IPropagateRoomBookingWorkflowIdTarget
{
    public async Task ApplyAsync(PropagateRoomBookingWorkflowIdRecord record, CancellationToken token)
    {
        await client.PreviewClockEventAsync(new ApiTimekeeping.PreviewClockEventRequest { ClockEvent = new ApiTimekeeping.ClockEvent { Source = record.Title }, ClockEventId = record.RoomBookingId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.dispatch.v1.DispatchOrderQueryService.</summary>
internal sealed class PropagateRoomBookingWorkflowIdDispatchOrderQueryRelay : IPropagateRoomBookingWorkflowIdTarget
{
    private readonly ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient _client;

    public PropagateRoomBookingWorkflowIdDispatchOrderQueryRelay(ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateRoomBookingWorkflowIdRecord record, CancellationToken token)
    {
        await _client.PeekDispatchOrderAsync(new ApiDispatch.PeekDispatchOrderRequest { DispatchOrderId = record.Title }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.invoicing.v1.InvoiceBatchCalculationService.</summary>
internal sealed class PropagateRoomBookingWorkflowIdInvoiceBatchCalculationSink(ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient client) : IPropagateRoomBookingWorkflowIdTarget
{
    public async Task ApplyAsync(PropagateRoomBookingWorkflowIdRecord record, CancellationToken token)
    {
        await client.RecalculateInvoiceBatchesAsync(new ApiInvoicing.RecalculateInvoiceBatchesRequest { InvoiceBatch = new ApiInvoicing.InvoiceBatch { Notes = record.Title } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.RoomBookingWorkflowService/ResumeRoomBookingWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateRoomBookingWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient(channel);
        var response = await source.ResumeRoomBookingWorkflowAsync(new ApiWorkplaces.ResumeRoomBookingWorkflowRequest() { RoomBookingId = "nightly-propagate-room-booking-workflow-id" }, cancellationToken: token);
        var record = new PropagateRoomBookingWorkflowIdRecord
        {
            RoomBookingId = response.RoomBooking.RoomBookingId,
            Title = response.RoomBooking.Title,
        };
        var sinks = new Dictionary<string, IPropagateRoomBookingWorkflowIdTarget>
        {
            ["clock-event-validation"] = new PropagateRoomBookingWorkflowIdClockEventValidationSink(new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(channel)),
            ["dispatch-order-query"] = new PropagateRoomBookingWorkflowIdDispatchOrderQueryRelay(new ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient(channel)),
            ["invoice-batch-calculation"] = new PropagateRoomBookingWorkflowIdInvoiceBatchCalculationSink(new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "clock-event-validation")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
