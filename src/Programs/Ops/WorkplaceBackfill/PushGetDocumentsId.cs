using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where PushGetDocumentsId hands each record.</summary>
internal interface IPushGetDocumentsIdTarget
{
    Task ForwardAsync(PushGetDocumentsIdRecord record, CancellationToken token);
}

/// <summary>What PushGetDocumentsId keeps of each BatchGetDocuments response.</summary>
internal sealed class PushGetDocumentsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Passes records to acme.surveys.v1.SurveyService.</summary>
internal sealed class PushGetDocumentsIdSurveySink : IPushGetDocumentsIdTarget
{
    private readonly ApiSurveys.SurveyService.SurveyServiceClient _client;

    public PushGetDocumentsIdSurveySink(ApiSurveys.SurveyService.SurveyServiceClient client) => _client = client;

    public async Task ForwardAsync(PushGetDocumentsIdRecord record, CancellationToken token)
    {
        await _client.CloneSurveyAsync(new ApiSurveys.CloneSurveyRequest { Survey = new ApiSurveys.Survey { Number = record.Message, SurveyId = record.Status }, RequestId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.boxoffice.v1.BoxOfficeSaleQueryService.</summary>
internal sealed class PushGetDocumentsIdBoxOfficeSaleQueryWriter(ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient client) : IPushGetDocumentsIdTarget
{
    public async Task ForwardAsync(PushGetDocumentsIdRecord record, CancellationToken token)
    {
        await client.SuggestBoxOfficeSalesAsync(new ApiBoxoffice.SuggestBoxOfficeSalesRequest { OrderBy = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushGetDocumentsIdLogOnly : IPushGetDocumentsIdTarget
{
    public Task ForwardAsync(PushGetDocumentsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.DocumentService/BatchGetDocuments and hands what it keeps to the sink of its kind.</summary>
internal static class PushGetDocumentsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentService.DocumentServiceClient(channel);
        var found = await source.BatchGetDocumentsAsync(new ApiDocuments.BatchGetDocumentsRequest(), cancellationToken: token);
        var record = new PushGetDocumentsIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Message = Text.Normalize(found.Result.Message),
            Status = found.Result.Status,
            NextPageToken = found.PageInfo.NextPageToken,
        };
        await DeliverPushGetDocumentsIdAsync("box-office-sale-query", record, channel, token);
    }

    private static async Task DeliverPushGetDocumentsIdAsync(string kind, PushGetDocumentsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPushGetDocumentsIdTarget sink = new PushGetDocumentsIdLogOnly();
        switch (kind)
        {
            case "survey":
                sink = new PushGetDocumentsIdSurveySink(new ApiSurveys.SurveyService.SurveyServiceClient(channel));
                break;
            case "box-office-sale-query":
                sink = new PushGetDocumentsIdBoxOfficeSaleQueryWriter(new ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
