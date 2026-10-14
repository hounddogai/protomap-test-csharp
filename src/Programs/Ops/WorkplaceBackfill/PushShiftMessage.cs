using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where PushShiftMessage hands each record.</summary>
internal interface IPushShiftMessageSink
{
    Task PushAsync(PushShiftMessageRecord record, CancellationToken token);
}

/// <summary>What PushShiftMessage keeps of each ApproveShift response.</summary>
internal sealed class PushShiftMessageRecord
{
    public string Message { get; init; } = "";
    public string ResultMessage { get; init; } = "";
}

/// <summary>Sends records to acme.messaging.v1.ConversationCalculationService.</summary>
internal sealed class PushShiftMessageConversationCalculationTarget : IPushShiftMessageSink
{
    private readonly ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient _client;

    public PushShiftMessageConversationCalculationTarget(ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient client) => _client = client;

    public async Task PushAsync(PushShiftMessageRecord record, CancellationToken token)
    {
        await _client.GetConversationRatesAsync(new ApiMessaging.GetConversationRatesRequest { ConversationId = record.ResultMessage }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.devices.v1.ManagedDeviceCalculationService.</summary>
internal sealed class PushShiftMessageManagedDeviceCalculationSink(ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient client) : IPushShiftMessageSink
{
    public async Task PushAsync(PushShiftMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.CalculateManagedDeviceRequest { ManagedDeviceId = record.Message, Reason = record.ResultMessage }, token, client.CalculateManagedDeviceAsync);
    }
}

/// <summary>Reads acme.scheduling.v1.ShiftApprovalService/ApproveShift and hands what it keeps to the sink of its kind.</summary>
internal static class PushShiftMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient(channel);
        var got = await source.ApproveShiftAsync(new ApiScheduling.ApproveShiftRequest() { Reason = "live-push-shift-message" }, cancellationToken: token);
        var record = new PushShiftMessageRecord
        {
            Message = got.Outcome.Message,
            ResultMessage = Text.Normalize(got.Result.Message),
        };
        var sinks = new List<IPushShiftMessageSink>
        {
            new PushShiftMessageConversationCalculationTarget(new ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient(channel)),
            new PushShiftMessageManagedDeviceCalculationSink(new ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.PushAsync(record, token);
    }
}
