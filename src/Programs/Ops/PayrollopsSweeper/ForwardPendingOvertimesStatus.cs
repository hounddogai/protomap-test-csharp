using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ForwardPendingOvertimesStatus hands each record.</summary>
internal interface IForwardPendingOvertimesStatusWriter
{
    Task ForwardAsync(ForwardPendingOvertimesStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardPendingOvertimesStatus keeps of each ListPendingOvertimes response.</summary>
internal sealed class ForwardPendingOvertimesStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.ShiftApprovalService.</summary>
internal sealed class ForwardPendingOvertimesStatusShiftApprovalSink : IForwardPendingOvertimesStatusWriter
{
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _client;

    public ForwardPendingOvertimesStatusShiftApprovalSink(ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardPendingOvertimesStatusRecord record, CancellationToken token)
    {
        await _client.ListPendingShiftsAsync(new ApiScheduling.ListPendingShiftsRequest { OrderBy = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.corporateevents.v1.CorporateEventApprovalService.</summary>
internal sealed class ForwardPendingOvertimesStatusCorporateEventApprovalSink : IForwardPendingOvertimesStatusWriter
{
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _client;

    public ForwardPendingOvertimesStatusCorporateEventApprovalSink(ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardPendingOvertimesStatusRecord record, CancellationToken token)
    {
        await _client.EscalateCorporateEventAsync(new ApiCorporateevents.EscalateCorporateEventRequest { Details = new ApiCorporateevents.EscalateCorporateEventRequest.Types.Details { ApproverEmail = record.CorrelationId }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.ManagedDeviceQueryService.</summary>
internal sealed class ForwardPendingOvertimesStatusManagedDeviceQueryWriter(ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient client) : IForwardPendingOvertimesStatusWriter
{
    public async Task ForwardAsync(ForwardPendingOvertimesStatusRecord record, CancellationToken token)
    {
        await client.PeekManagedDeviceAsync(new ApiDevices.PeekManagedDeviceRequest { ManagedDeviceId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardPendingOvertimesStatusLogOnly : IForwardPendingOvertimesStatusWriter
{
    public Task ForwardAsync(ForwardPendingOvertimesStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.OvertimeApprovalService/ListPendingOvertimes and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardPendingOvertimesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient(channel);
        var found = await source.ListPendingOvertimesAsync(new ApiTimekeeping.ListPendingOvertimesRequest() { OrderBy = "live-forward-pending-overtimes-status" }, cancellationToken: token);
        var record = new ForwardPendingOvertimesStatusRecord
        {
            Status = Text.Normalize(found.Result.Status),
            CorrelationId = found.Result.CorrelationId,
        };
        await DeliverForwardPendingOvertimesStatusAsync("corporate-event-approval", record, channel, token);
    }

    private static async Task DeliverForwardPendingOvertimesStatusAsync(string kind, ForwardPendingOvertimesStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IForwardPendingOvertimesStatusWriter sink = kind switch
        {
            "shift-approval" => new ForwardPendingOvertimesStatusShiftApprovalSink(new ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient(channel)),
            "corporate-event-approval" => new ForwardPendingOvertimesStatusCorporateEventApprovalSink(new ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient(channel)),
            "managed-device-query" => new ForwardPendingOvertimesStatusManagedDeviceQueryWriter(new ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient(channel)),
            _ => new ForwardPendingOvertimesStatusLogOnly(),
        };
        await sink.ForwardAsync(record, token);
    }
}
