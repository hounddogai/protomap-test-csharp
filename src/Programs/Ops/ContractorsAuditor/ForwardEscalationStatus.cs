using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where ForwardEscalationStatus hands each record.</summary>
internal interface IForwardEscalationStatusSink
{
    Task PushAsync(ForwardEscalationStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardEscalationStatus keeps of each QuoteEscalation response.</summary>
internal sealed class ForwardEscalationStatusRecord
{
    public string Status { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Sends records to acme.devices.v1.ManagedDeviceQueryService.</summary>
internal sealed class ForwardEscalationStatusManagedDeviceQueryRelay(ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient client) : IForwardEscalationStatusSink
{
    public async Task PushAsync(ForwardEscalationStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.PeekManagedDeviceRequest { ManagedDeviceId = record.Status }, token, client.PeekManagedDeviceAsync);
    }
}

/// <summary>Passes records to acme.corporateevents.v1.CorporateEventWorkflowService.</summary>
internal sealed class ForwardEscalationStatusCorporateEventWorkflowSink : IForwardEscalationStatusSink
{
    private readonly ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient _client;

    public ForwardEscalationStatusCorporateEventWorkflowSink(ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient client) => _client = client;

    public async Task PushAsync(ForwardEscalationStatusRecord record, CancellationToken token)
    {
        await _client.RetryCorporateEventWorkflowAsync(new ApiCorporateevents.RetryCorporateEventWorkflowRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.travel.v1.TravelBookingQueryService.</summary>
internal sealed class ForwardEscalationStatusTravelBookingQueryWriter(ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient client) : IForwardEscalationStatusSink
{
    public async Task PushAsync(ForwardEscalationStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.CountTravelBookingsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status } }, token, client.CountTravelBookingsAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardEscalationStatusLogOnly : IForwardEscalationStatusSink
{
    public Task PushAsync(ForwardEscalationStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.EscalationCalculationService/QuoteEscalation and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardEscalationStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient(channel);
        var reply = await source.QuoteEscalationAsync(new ApiHelpdesk.QuoteEscalationRequest() { Reason = "default-forward-escalation-status" }, cancellationToken: token);
        var record = new ForwardEscalationStatusRecord
        {
            Status = reply.Result.Status,
            Subject = reply.Escalation.Subject,
        };
        await DispatchForwardEscalationStatusAsync("managed-device-query", record, channel, token);
    }

    private static async Task DispatchForwardEscalationStatusAsync(string kind, ForwardEscalationStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<ForwardEscalationStatusRecord, CancellationToken, Task> write = new ForwardEscalationStatusLogOnly().PushAsync;
        switch (kind)
        {
            case "managed-device-query":
                write = new ForwardEscalationStatusManagedDeviceQueryRelay(new ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient(channel)).PushAsync;
                break;
            case "corporate-event-workflow":
                write = new ForwardEscalationStatusCorporateEventWorkflowSink(new ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient(channel)).PushAsync;
                break;
            case "travel-booking-query":
                write = new ForwardEscalationStatusTravelBookingQueryWriter(new ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient(channel)).PushAsync;
                break;
        }
        await write(record, token);
    }
}
