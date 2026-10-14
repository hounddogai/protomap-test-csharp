using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where ExportCorporateEventMessage hands each record.</summary>
internal interface IExportCorporateEventMessageWriter
{
    Task ApplyAsync(ExportCorporateEventMessageRecord record, CancellationToken token);
}

/// <summary>What ExportCorporateEventMessage keeps of each ValidateCorporateEvent response.</summary>
internal sealed class ExportCorporateEventMessageRecord
{
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
    public string CorporateEventId { get; init; } = "";
}

/// <summary>Writes records to acme.helpdesk.v1.KnowledgeArticleValidationService.</summary>
internal sealed class ExportCorporateEventMessageKnowledgeArticleValidationRelay(ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient client) : IExportCorporateEventMessageWriter
{
    public async Task ApplyAsync(ExportCorporateEventMessageRecord record, CancellationToken token)
    {
        await client.ScoreKnowledgeArticleAsync(new ApiHelpdesk.ScoreKnowledgeArticleRequest { Details = new ApiHelpdesk.ScoreKnowledgeArticleRequest.Types.Details { CurrencyCode = record.CorporateEventId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.learning.v1.LearningPathQueryService.</summary>
internal sealed class ExportCorporateEventMessageLearningPathQueryWriter(ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient client) : IExportCorporateEventMessageWriter
{
    public async Task ApplyAsync(ExportCorporateEventMessageRecord record, CancellationToken token)
    {
        await client.SummarizeLearningPathsAsync(new ApiLearning.SummarizeLearningPathsRequest { Details = new ApiLearning.SummarizeLearningPathsRequest.Types.Details { Cron = record.CorporateEventId }, LearningPathId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventValidationService/ValidateCorporateEvent and hands what it keeps to the sink of its kind.</summary>
internal static class ExportCorporateEventMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient(channel);
        var found = await source.ValidateCorporateEventAsync(new ApiCorporateevents.ValidateCorporateEventRequest() { Reason = "live-export-corporate-event-message" }, cancellationToken: token);
        var record = new ExportCorporateEventMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            ReferenceId = found.Outcome.ReferenceId,
            OutcomeMessage = found.Outcome.Message,
            CorporateEventId = Text.Normalize(found.CorporateEvent.CorporateEventId),
        };
        var sinks = new List<IExportCorporateEventMessageWriter>
        {
            new ExportCorporateEventMessageKnowledgeArticleValidationRelay(new ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient(channel)),
            new ExportCorporateEventMessageLearningPathQueryWriter(new ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.ApplyAsync(record, token);
    }
}
