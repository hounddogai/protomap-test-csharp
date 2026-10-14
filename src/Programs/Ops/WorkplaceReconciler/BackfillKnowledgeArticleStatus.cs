using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where BackfillKnowledgeArticleStatus hands each record.</summary>
internal interface IBackfillKnowledgeArticleStatusDestination
{
    Task ForwardAsync(BackfillKnowledgeArticleStatusRecord record, CancellationToken token);
}

/// <summary>What BackfillKnowledgeArticleStatus keeps of each ApproveKnowledgeArticle response.</summary>
internal sealed class BackfillKnowledgeArticleStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Sends records to acme.corporateevents.v1.VenueCalculationService.</summary>
internal sealed class BackfillKnowledgeArticleStatusVenueCalculationTarget(ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient client) : IBackfillKnowledgeArticleStatusDestination
{
    public async Task ForwardAsync(BackfillKnowledgeArticleStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.CompareVenueOptionsRequest { Details = new ApiCorporateevents.CompareVenueOptionsRequest.Types.Details { CurrencyCode = record.Status }, Venue = new ApiCorporateevents.Venue { Code = record.ReferenceId, Name = record.Notes } }, token, client.CompareVenueOptionsAsync);
    }
}

/// <summary>Sends records to acme.onboarding.v1.EquipmentReportingService.</summary>
internal sealed class BackfillKnowledgeArticleStatusEquipmentReportingSink : IBackfillKnowledgeArticleStatusDestination
{
    private readonly ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient _client;

    public BackfillKnowledgeArticleStatusEquipmentReportingSink(ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillKnowledgeArticleStatusRecord record, CancellationToken token)
    {
        await _client.DeleteEquipmentReportAsync(new ApiOnboarding.DeleteEquipmentReportRequest { EquipmentId = record.ReferenceId, Etag = record.Notes, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.helpdesk.v1.EscalationValidationService.</summary>
internal sealed class BackfillKnowledgeArticleStatusEscalationValidationWriter(ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient client) : IBackfillKnowledgeArticleStatusDestination
{
    public async Task ForwardAsync(BackfillKnowledgeArticleStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.CheckEscalationEligibilityRequest { Details = new ApiHelpdesk.CheckEscalationEligibilityRequest.Types.Details { Locale = record.ReferenceId }, Escalation = new ApiHelpdesk.Escalation { EscalationId = record.Status }, EscalationId = record.Notes }, token, client.CheckEscalationEligibilityAsync);
    }
}

/// <summary>Reads acme.helpdesk.v1.KnowledgeArticleApprovalService/ApproveKnowledgeArticle and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillKnowledgeArticleStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient(channel);
        var reply = await source.ApproveKnowledgeArticleAsync(new ApiHelpdesk.ApproveKnowledgeArticleRequest() { Reason = "main-backfill-knowledge-article-status" }, cancellationToken: token);
        var record = new BackfillKnowledgeArticleStatusRecord
        {
            Status = reply.Outcome.Status,
            ReferenceId = reply.Outcome.ReferenceId,
            Notes = reply.KnowledgeArticle.Notes,
        };
        var sinks = new Dictionary<string, IBackfillKnowledgeArticleStatusDestination>
        {
            ["venue-calculation"] = new BackfillKnowledgeArticleStatusVenueCalculationTarget(new ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient(channel)),
            ["equipment-reporting"] = new BackfillKnowledgeArticleStatusEquipmentReportingSink(new ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient(channel)),
            ["escalation-validation"] = new BackfillKnowledgeArticleStatusEscalationValidationWriter(new ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient(channel)),
        };
        await RouteBackfillKnowledgeArticleStatusAsync("equipment-reporting", record, sinks, token);
    }

    private static async Task RouteBackfillKnowledgeArticleStatusAsync(string kind, BackfillKnowledgeArticleStatusRecord record, IReadOnlyDictionary<string, IBackfillKnowledgeArticleStatusDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
