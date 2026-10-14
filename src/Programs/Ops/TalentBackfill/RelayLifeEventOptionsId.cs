using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>What RelayLifeEventOptionsId keeps of each CompareLifeEventOptions response.</summary>
internal sealed class RelayLifeEventOptionsIdRecord
{
    public string LifeEventId { get; init; } = "";
    public string Source { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.SignatureValidationService.</summary>
internal sealed class RelayLifeEventOptionsIdSignatureValidationSink(ApiDocuments.SignatureValidationService.SignatureValidationServiceClient client)
{
    public async Task SendAsync(RelayLifeEventOptionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.ExplainSignatureDecisionRequest { Details = new ApiDocuments.ExplainSignatureDecisionRequest.Types.Details { Locale = record.Source }, Reason = record.LifeEventId }, token, client.ExplainSignatureDecisionAsync);
    }
}

/// <summary>Reads acme.benefits.v1.LifeEventCalculationService/CompareLifeEventOptions and hands what it keeps to acme.documents.v1.SignatureValidationService.</summary>
internal static class RelayLifeEventOptionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient(channel);
        var response = await source.CompareLifeEventOptionsAsync(new ApiBenefits.CompareLifeEventOptionsRequest() { Reason = "current-relay-life-event-options-id" }, cancellationToken: token);
        var record = new RelayLifeEventOptionsIdRecord
        {
            LifeEventId = Text.Normalize(response.LifeEvent.LifeEventId),
            Source = Text.Normalize(response.LifeEvent.Source),
        };
        await new RelayLifeEventOptionsIdSignatureValidationSink(new ApiDocuments.SignatureValidationService.SignatureValidationServiceClient(channel)).SendAsync(record, token);
    }
}
