using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where ForwardMediaAssetId hands each record.</summary>
internal interface IForwardMediaAssetIdTarget
{
    Task ForwardAsync(ForwardMediaAssetIdRecord record, CancellationToken token);
}

/// <summary>What ForwardMediaAssetId keeps of each PeekMediaAsset response.</summary>
internal sealed class ForwardMediaAssetIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Hands records to acme.recruiting.v1.OfferCalculationService.</summary>
internal sealed class ForwardMediaAssetIdOfferCalculationWriter(ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient client) : IForwardMediaAssetIdTarget
{
    public async Task ForwardAsync(ForwardMediaAssetIdRecord record, CancellationToken token)
    {
        await client.SimulateOfferAsync(new ApiRecruiting.SimulateOfferRequest { Details = new ApiRecruiting.SimulateOfferRequest.Types.Details { CurrencyCode = record.Name }, Offer = new ApiRecruiting.Offer { Notes = record.CorrelationId }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.documents.v1.DocumentTemplateSyncService.</summary>
internal sealed class ForwardMediaAssetIdDocumentTemplateSyncSink : IForwardMediaAssetIdTarget
{
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _client;

    public ForwardMediaAssetIdDocumentTemplateSyncSink(ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardMediaAssetIdRecord record, CancellationToken token)
    {
        await _client.ListDocumentTemplateConflictsAsync(new ApiDocuments.ListDocumentTemplateConflictsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter { Region = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.recruiting.v1.JobOpeningValidationService.</summary>
internal sealed class ForwardMediaAssetIdJobOpeningValidationTarget(ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient client) : IForwardMediaAssetIdTarget
{
    public async Task ForwardAsync(ForwardMediaAssetIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.CheckJobOpeningEligibilityRequest { JobOpening = new ApiRecruiting.JobOpening { Number = record.CorrelationId, JobOpeningId = record.Name }, Reason = record.Status }, token, client.CheckJobOpeningEligibilityAsync);
    }
}

/// <summary>Hands records to acme.itsm.v1.ChangeRequestValidationService.</summary>
internal sealed class ForwardMediaAssetIdChangeRequestValidationRelay(ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient client) : IForwardMediaAssetIdTarget
{
    public async Task ForwardAsync(ForwardMediaAssetIdRecord record, CancellationToken token)
    {
        await client.ExplainChangeRequestDecisionAsync(new ApiItsm.ExplainChangeRequestDecisionRequest { Details = new ApiItsm.ExplainChangeRequestDecisionRequest.Types.Details { Locale = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.media.v1.MediaAssetQueryService/PeekMediaAsset and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardMediaAssetId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient(channel);
        var reply = await source.PeekMediaAssetAsync(new ApiMedia.PeekMediaAssetRequest() { MediaAssetId = "current-forward-media-asset-id" }, cancellationToken: token);
        var record = new ForwardMediaAssetIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Message = reply.Result.Message,
            Status = reply.Result.Status,
            Name = reply.MediaAsset.Name,
        };
        var sinks = new List<IForwardMediaAssetIdTarget>
        {
            new ForwardMediaAssetIdOfferCalculationWriter(new ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient(channel)),
            new ForwardMediaAssetIdDocumentTemplateSyncSink(new ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient(channel)),
            new ForwardMediaAssetIdJobOpeningValidationTarget(new ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient(channel)),
            new ForwardMediaAssetIdChangeRequestValidationRelay(new ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient(channel)),
        };
        await sinks[0].ForwardAsync(record, token);
    }
}
