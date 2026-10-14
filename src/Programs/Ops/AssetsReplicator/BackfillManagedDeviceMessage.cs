using Grpc.Net.Client;
using ApiDevices = Acme.Devices.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>What BackfillManagedDeviceMessage keeps of each DeleteManagedDevice response.</summary>
internal sealed class BackfillManagedDeviceMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.performance.v1.FeedbackApprovalService.</summary>
internal sealed class BackfillManagedDeviceMessageFeedbackApprovalSink(ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient client)
{
    public async Task WriteAsync(BackfillManagedDeviceMessageRecord record, CancellationToken token)
    {
        await client.ApproveFeedbackAsync(new ApiPerformance.ApproveFeedbackRequest { Feedback = new ApiPerformance.Feedback { Source = record.Message }, FeedbackId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.devices.v1.ManagedDeviceService/DeleteManagedDevice and hands what it keeps to acme.performance.v1.FeedbackApprovalService.</summary>
internal static class BackfillManagedDeviceMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient(channel);
        var got = await source.DeleteManagedDeviceAsync(new ApiDevices.DeleteManagedDeviceRequest() { Reason = "primary-backfill-managed-device-message" }, cancellationToken: token);
        var record = new BackfillManagedDeviceMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = got.Result.CorrelationId,
        };
        await new BackfillManagedDeviceMessageFeedbackApprovalSink(new ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient(channel)).WriteAsync(record, token);
    }
}
