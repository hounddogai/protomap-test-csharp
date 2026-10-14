using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>What MirrorDeskWorkflowId keeps of each ResumeDeskWorkflow response.</summary>
internal sealed class MirrorDeskWorkflowIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Writes records to acme.helpdesk.v1.KnowledgeArticleValidationService.</summary>
internal sealed class MirrorDeskWorkflowIdKnowledgeArticleValidationWriter(ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient client)
{
    public async Task ApplyAsync(MirrorDeskWorkflowIdRecord record, CancellationToken token)
    {
        await client.ScoreKnowledgeArticleAsync(new ApiHelpdesk.ScoreKnowledgeArticleRequest { KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle { Notes = record.Code }, Details = new ApiHelpdesk.ScoreKnowledgeArticleRequest.Types.Details { CurrencyCode = record.ReferenceId }, KnowledgeArticleId = record.Name }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.DeskWorkflowService/ResumeDeskWorkflow and hands what it keeps to acme.helpdesk.v1.KnowledgeArticleValidationService.</summary>
internal static class MirrorDeskWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient(channel);
        var reply = await source.ResumeDeskWorkflowAsync(new ApiWorkplaces.ResumeDeskWorkflowRequest() { Reason = "default-mirror-desk-workflow-id" }, cancellationToken: token);
        var record = new MirrorDeskWorkflowIdRecord
        {
            ReferenceId = reply.Outcome.ReferenceId,
            Code = reply.Desk.Code,
            Name = Text.Normalize(reply.Desk.Name),
        };
        await new MirrorDeskWorkflowIdKnowledgeArticleValidationWriter(new ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient(channel)).ApplyAsync(record, token);
    }
}
