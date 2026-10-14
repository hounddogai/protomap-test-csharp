using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where CarryIncidentsToken hands each record.</summary>
internal interface ICarryIncidentsTokenSink
{
    Task DeliverAsync(CarryIncidentsTokenRecord record, CancellationToken token);
}

/// <summary>What CarryIncidentsToken keeps of each ListIncidents response.</summary>
internal sealed class CarryIncidentsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentTemplateHistoryService.</summary>
internal sealed class CarryIncidentsTokenDocumentTemplateHistorySink : ICarryIncidentsTokenSink
{
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _client;

    public CarryIncidentsTokenDocumentTemplateHistorySink(ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient client) => _client = client;

    public async Task DeliverAsync(CarryIncidentsTokenRecord record, CancellationToken token)
    {
        await _client.GetDocumentTemplateVersionAsync(new ApiDocuments.GetDocumentTemplateVersionRequest { DocumentTemplateId = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.treasury.v1.CashPositionQueryService.</summary>
internal sealed class CarryIncidentsTokenCashPositionQuerySink(ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient client) : ICarryIncidentsTokenSink
{
    public async Task DeliverAsync(CarryIncidentsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTreasury.SuggestCashPositionsRequest { Filter = new ApiTreasury.SuggestCashPositionsRequest.Types.Filter { Region = record.CorrelationId }, OrderBy = record.NextPageToken }, token, client.SuggestCashPositionsAsync);
    }
}

/// <summary>Sends records to acme.boxoffice.v1.BoxOfficeSaleService.</summary>
internal sealed class CarryIncidentsTokenBoxOfficeSaleWriter : ICarryIncidentsTokenSink
{
    private readonly ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient _client;

    public CarryIncidentsTokenBoxOfficeSaleWriter(ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient client) => _client = client;

    public async Task DeliverAsync(CarryIncidentsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBoxoffice.GetBoxOfficeSaleRequest { BoxOfficeSaleId = record.CorrelationId }, token, _client.GetBoxOfficeSaleAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CarryIncidentsTokenLogOnly : ICarryIncidentsTokenSink
{
    public Task DeliverAsync(CarryIncidentsTokenRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.NextPageToken}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.itsm.v1.IncidentService/ListIncidents and hands what it keeps to the sink of its kind.</summary>
internal static class CarryIncidentsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.IncidentService.IncidentServiceClient(channel);
        var response = await source.ListIncidentsAsync(new ApiItsm.ListIncidentsRequest() { OrderBy = "default-carry-incidents-token" }, cancellationToken: token);
        var record = new CarryIncidentsTokenRecord
        {
            NextPageToken = response.PageInfo.NextPageToken,
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
        };
        await RouteCarryIncidentsTokenAsync("box-office-sale", record, channel, token);
    }

    private static async Task RouteCarryIncidentsTokenAsync(string kind, CarryIncidentsTokenRecord record, GrpcChannel channel, CancellationToken token)
    {
        ICarryIncidentsTokenSink sink = new CarryIncidentsTokenLogOnly();
        switch (kind)
        {
            case "document-template-history":
                sink = new CarryIncidentsTokenDocumentTemplateHistorySink(new ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient(channel));
                break;
            case "cash-position-query":
                sink = new CarryIncidentsTokenCashPositionQuerySink(new ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient(channel));
                break;
            case "box-office-sale":
                sink = new CarryIncidentsTokenBoxOfficeSaleWriter(new ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient(channel));
                break;
        }
        await sink.DeliverAsync(record, token);
    }
}
