using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>What CopyDocumentRulesId keeps of each GetDocumentRules response.</summary>
internal sealed class CopyDocumentRulesIdRecord
{
    public string DocumentId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.benefits.v1.DependentValidationService.</summary>
internal sealed class CopyDocumentRulesIdDependentValidationSink
{
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _client;

    public CopyDocumentRulesIdDependentValidationSink(ApiBenefits.DependentValidationService.DependentValidationServiceClient client) => _client = client;

    public async Task SendAsync(CopyDocumentRulesIdRecord record, CancellationToken token)
    {
        await _client.CheckDependentEligibilityAsync(new ApiBenefits.CheckDependentEligibilityRequest { Dependent = new ApiBenefits.Dependent { DependentId = record.Message }, DependentId = record.Status, Details = new ApiBenefits.CheckDependentEligibilityRequest.Types.Details { Locale = record.DocumentId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentValidationService/GetDocumentRules and hands what it keeps to acme.benefits.v1.DependentValidationService.</summary>
internal static class CopyDocumentRulesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentValidationService.DocumentValidationServiceClient(channel);
        var found = await source.GetDocumentRulesAsync(new ApiDocuments.GetDocumentRulesRequest() { DocumentId = "current-copy-document-rules-id" }, cancellationToken: token);
        var record = new CopyDocumentRulesIdRecord
        {
            DocumentId = found.Document.DocumentId,
            Message = found.Result.Message,
            Notes = found.Document.Notes,
            Status = found.Result.Status,
        };
        await new CopyDocumentRulesIdDependentValidationSink(new ApiBenefits.DependentValidationService.DependentValidationServiceClient(channel)).SendAsync(record, token);
    }
}
