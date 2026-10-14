using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where PropagatePendingLearningPathsToken hands each record.</summary>
internal interface IPropagatePendingLearningPathsTokenWriter
{
    Task WriteAsync(PropagatePendingLearningPathsTokenRecord record, CancellationToken token);
}

/// <summary>What PropagatePendingLearningPathsToken keeps of each ListPendingLearningPaths response.</summary>
internal sealed class PropagatePendingLearningPathsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.keymanagement.v1.CryptoKeyQueryService.</summary>
internal sealed class PropagatePendingLearningPathsTokenCryptoKeyQueryTarget(ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient client) : IPropagatePendingLearningPathsTokenWriter
{
    public async Task WriteAsync(PropagatePendingLearningPathsTokenRecord record, CancellationToken token)
    {
        await client.SuggestCryptoKeysAsync(new ApiKeymanagement.SuggestCryptoKeysRequest { Filter = new ApiKeymanagement.SuggestCryptoKeysRequest.Types.Filter { OwnerId = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.NextPageToken }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.devices.v1.ManagedDeviceApprovalService.</summary>
internal sealed class PropagatePendingLearningPathsTokenManagedDeviceApprovalRelay(ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient client) : IPropagatePendingLearningPathsTokenWriter
{
    public async Task WriteAsync(PropagatePendingLearningPathsTokenRecord record, CancellationToken token)
    {
        await client.GetManagedDeviceApprovalStatusAsync(new ApiDevices.GetManagedDeviceApprovalStatusRequest { ManagedDeviceId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.LearningPathApprovalService/ListPendingLearningPaths and hands what it keeps to the sink of its kind.</summary>
internal static class PropagatePendingLearningPathsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient(channel);
        var reply = await source.ListPendingLearningPathsAsync(new ApiLearning.ListPendingLearningPathsRequest() { OrderBy = "nightly-propagate-pending-learning-paths-token" }, cancellationToken: token);
        var record = new PropagatePendingLearningPathsTokenRecord
        {
            NextPageToken = reply.PageInfo.NextPageToken,
            Message = Text.Normalize(reply.Result.Message),
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
        };
        var sinks = new Dictionary<string, IPropagatePendingLearningPathsTokenWriter>
        {
            ["crypto-key-query"] = new PropagatePendingLearningPathsTokenCryptoKeyQueryTarget(new ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient(channel)),
            ["managed-device-approval"] = new PropagatePendingLearningPathsTokenManagedDeviceApprovalRelay(new ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient(channel)),
        };
        await RoutePropagatePendingLearningPathsTokenAsync("crypto-key-query", record, sinks, token);
    }

    private static async Task RoutePropagatePendingLearningPathsTokenAsync(string kind, PropagatePendingLearningPathsTokenRecord record, IReadOnlyDictionary<string, IPropagatePendingLearningPathsTokenWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
