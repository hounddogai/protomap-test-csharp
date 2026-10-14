using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiMedia = Acme.Media.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where MirrorFeatureViewStatus hands each record.</summary>
internal interface IMirrorFeatureViewStatusTarget
{
    Task SendAsync(MirrorFeatureViewStatusRecord record, CancellationToken token);
}

/// <summary>What MirrorFeatureViewStatus keeps of each SimulateFeatureView response.</summary>
internal sealed class MirrorFeatureViewStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.media.v1.MediaAssetCalculationService.</summary>
internal sealed class MirrorFeatureViewStatusMediaAssetCalculationTarget : IMirrorFeatureViewStatusTarget
{
    private readonly ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient _client;

    public MirrorFeatureViewStatusMediaAssetCalculationTarget(ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient client) => _client = client;

    public async Task SendAsync(MirrorFeatureViewStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiMedia.EstimateMediaAssetRequest { MediaAsset = new ApiMedia.MediaAsset { MediaAssetId = record.Status, Name = record.Message } }, token, _client.EstimateMediaAssetAsync);
    }
}

/// <summary>Writes records to acme.benefits.v1.LifeEventValidationService.</summary>
internal sealed class MirrorFeatureViewStatusLifeEventValidationRelay : IMirrorFeatureViewStatusTarget
{
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _client;

    public MirrorFeatureViewStatusLifeEventValidationRelay(ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient client) => _client = client;

    public async Task SendAsync(MirrorFeatureViewStatusRecord record, CancellationToken token)
    {
        await _client.ExplainLifeEventDecisionAsync(new ApiBenefits.ExplainLifeEventDecisionRequest { Details = new ApiBenefits.ExplainLifeEventDecisionRequest.Types.Details { Locale = record.Message }, LifeEvent = new ApiBenefits.LifeEvent { Kind = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.yards.v1.YardQueryService.</summary>
internal sealed class MirrorFeatureViewStatusYardQueryWriter(ApiYards.YardQueryService.YardQueryServiceClient client) : IMirrorFeatureViewStatusTarget
{
    public async Task SendAsync(MirrorFeatureViewStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiYards.LookupYardRequest { YardId = record.Message }, token, client.LookupYardAsync);
    }
}

/// <summary>Reads acme.featurestore.v1.FeatureViewCalculationService/SimulateFeatureView and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorFeatureViewStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(channel);
        var reply = await source.SimulateFeatureViewAsync(new ApiFeaturestore.SimulateFeatureViewRequest() { Reason = "main-mirror-feature-view-status" }, cancellationToken: token);
        var record = new MirrorFeatureViewStatusRecord
        {
            Status = Text.Normalize(reply.Result.Status),
            Message = reply.Result.Message,
        };
        await ActOnMirrorFeatureViewStatusAsync("life-event-validation", record, channel, token);
    }

    private static async Task ActOnMirrorFeatureViewStatusAsync(string kind, MirrorFeatureViewStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["media-asset-calculation"] = () => new MirrorFeatureViewStatusMediaAssetCalculationTarget(new ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient(channel)).SendAsync(record, token),
            ["life-event-validation"] = () => new MirrorFeatureViewStatusLifeEventValidationRelay(new ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient(channel)).SendAsync(record, token),
            ["yard-query"] = () => new MirrorFeatureViewStatusYardQueryWriter(new ApiYards.YardQueryService.YardQueryServiceClient(channel)).SendAsync(record, token),
        };
        await actions[kind]();
    }
}
