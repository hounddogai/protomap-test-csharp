using Grpc.Net.Client;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>What MirrorProblemConflictId keeps of each ResolveProblemConflict response.</summary>
internal sealed class MirrorProblemConflictIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string ProblemId { get; init; } = "";
}

/// <summary>Passes records to acme.itsm.v1.ServiceAssetLifecycleService.</summary>
internal sealed class MirrorProblemConflictIdServiceAssetLifecycleTarget(ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient client)
{
    public async Task WriteAsync(MirrorProblemConflictIdRecord record, CancellationToken token)
    {
        await client.ScheduleServiceAssetExpiryAsync(new ApiItsm.ScheduleServiceAssetExpiryRequest { Reason = record.Message, Details = new ApiItsm.ScheduleServiceAssetExpiryRequest.Types.Details { InitiatedBy = record.ProblemId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ProblemSyncService/ResolveProblemConflict and hands what it keeps to acme.itsm.v1.ServiceAssetLifecycleService.</summary>
internal static class MirrorProblemConflictId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ProblemSyncService.ProblemSyncServiceClient(channel);
        var got = await source.ResolveProblemConflictAsync(new ApiItsm.ResolveProblemConflictRequest() { Reason = "live-mirror-problem-conflict-id" }, cancellationToken: token);
        var record = new MirrorProblemConflictIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            Message = got.Result.Message,
            ProblemId = got.Problem.ProblemId,
        };
        await new MirrorProblemConflictIdServiceAssetLifecycleTarget(new ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient(channel)).WriteAsync(record, token);
    }
}
