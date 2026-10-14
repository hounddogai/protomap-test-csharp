using Grpc.Net.Client;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where ForwardDocumentTemplateRatesStatus hands each record.</summary>
internal interface IForwardDocumentTemplateRatesStatusDestination
{
    Task ForwardAsync(ForwardDocumentTemplateRatesStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardDocumentTemplateRatesStatus keeps of each GetDocumentTemplateRates response.</summary>
internal sealed class ForwardDocumentTemplateRatesStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string DocumentTemplateId { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.SwapRequestValidationService.</summary>
internal sealed class ForwardDocumentTemplateRatesStatusSwapRequestValidationTarget : IForwardDocumentTemplateRatesStatusDestination
{
    private readonly ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient _client;

    public ForwardDocumentTemplateRatesStatusSwapRequestValidationTarget(ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardDocumentTemplateRatesStatusRecord record, CancellationToken token)
    {
        await _client.ExplainSwapRequestDecisionAsync(new ApiScheduling.ExplainSwapRequestDecisionRequest { SwapRequest = new ApiScheduling.SwapRequest { SwapRequestId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentAdminService.</summary>
internal sealed class ForwardDocumentTemplateRatesStatusDocumentAdminSink : IForwardDocumentTemplateRatesStatusDestination
{
    private readonly ApiDocuments.DocumentAdminService.DocumentAdminServiceClient _client;

    public ForwardDocumentTemplateRatesStatusDocumentAdminSink(ApiDocuments.DocumentAdminService.DocumentAdminServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardDocumentTemplateRatesStatusRecord record, CancellationToken token)
    {
        await _client.LockDocumentAsync(new ApiDocuments.LockDocumentRequest { Details = new ApiDocuments.LockDocumentRequest.Types.Details { ReasonCode = record.Status }, Reason = record.DocumentTemplateId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.corporateevents.v1.VenueValidationService.</summary>
internal sealed class ForwardDocumentTemplateRatesStatusVenueValidationTarget : IForwardDocumentTemplateRatesStatusDestination
{
    private readonly ApiCorporateevents.VenueValidationService.VenueValidationServiceClient _client;

    public ForwardDocumentTemplateRatesStatusVenueValidationTarget(ApiCorporateevents.VenueValidationService.VenueValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardDocumentTemplateRatesStatusRecord record, CancellationToken token)
    {
        await _client.GetVenueRulesAsync(new ApiCorporateevents.GetVenueRulesRequest { VenueId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardDocumentTemplateRatesStatusLogOnly : IForwardDocumentTemplateRatesStatusDestination
{
    public Task ForwardAsync(ForwardDocumentTemplateRatesStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.DocumentTemplateCalculationService/GetDocumentTemplateRates and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardDocumentTemplateRatesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient(channel);
        var reply = await source.GetDocumentTemplateRatesAsync(new ApiDocuments.GetDocumentTemplateRatesRequest() { DocumentTemplateId = "nightly-forward-document-template-rates-status" }, cancellationToken: token);
        var record = new ForwardDocumentTemplateRatesStatusRecord
        {
            Status = reply.Result.Status,
            Message = reply.Result.Message,
            DocumentTemplateId = reply.DocumentTemplate.DocumentTemplateId,
        };
        await DeliverForwardDocumentTemplateRatesStatusAsync("document-admin", record, channel, token);
    }

    private static async Task DeliverForwardDocumentTemplateRatesStatusAsync(string kind, ForwardDocumentTemplateRatesStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IForwardDocumentTemplateRatesStatusDestination sink = kind switch
        {
            "swap-request-validation" => new ForwardDocumentTemplateRatesStatusSwapRequestValidationTarget(new ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient(channel)),
            "document-admin" => new ForwardDocumentTemplateRatesStatusDocumentAdminSink(new ApiDocuments.DocumentAdminService.DocumentAdminServiceClient(channel)),
            "venue-validation" => new ForwardDocumentTemplateRatesStatusVenueValidationTarget(new ApiCorporateevents.VenueValidationService.VenueValidationServiceClient(channel)),
            _ => new ForwardDocumentTemplateRatesStatusLogOnly(),
        };
        await sink.ForwardAsync(record, token);
    }
}
