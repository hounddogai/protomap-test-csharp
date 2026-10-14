using Grpc.Net.Client;
using ApiBanking = Acme.Banking.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where RelayOrgUnitsStatus hands each record.</summary>
internal interface IRelayOrgUnitsStatusDestination
{
    Task SendAsync(RelayOrgUnitsStatusRecord record, CancellationToken token);
}

/// <summary>What RelayOrgUnitsStatus keeps of each FindOrgUnits response.</summary>
internal sealed class RelayOrgUnitsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Writes records to acme.search.v1.SearchIndexQueryService.</summary>
internal sealed class RelayOrgUnitsStatusSearchIndexQueryWriter(ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient client) : IRelayOrgUnitsStatusDestination
{
    public async Task SendAsync(RelayOrgUnitsStatusRecord record, CancellationToken token)
    {
        await client.SuggestSearchIndexesAsync(new ApiSearch.SuggestSearchIndexesRequest { OrderBy = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.banking.v1.BankAccountQueryService.</summary>
internal sealed class RelayOrgUnitsStatusBankAccountQuerySink : IRelayOrgUnitsStatusDestination
{
    private readonly ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient _client;

    public RelayOrgUnitsStatusBankAccountQuerySink(ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient client) => _client = client;

    public async Task SendAsync(RelayOrgUnitsStatusRecord record, CancellationToken token)
    {
        await _client.BrowseBankAccountsAsync(new ApiBanking.BrowseBankAccountsRequest { OrderBy = record.Message, Filter = new ApiBanking.BrowseBankAccountsRequest.Types.Filter { Region = record.NextPageToken, Query = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.benefits.v1.EnrollmentQueryService.</summary>
internal sealed class RelayOrgUnitsStatusEnrollmentQueryRelay : IRelayOrgUnitsStatusDestination
{
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _client;

    public RelayOrgUnitsStatusEnrollmentQueryRelay(ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient client) => _client = client;

    public async Task SendAsync(RelayOrgUnitsStatusRecord record, CancellationToken token)
    {
        await _client.SuggestEnrollmentsAsync(new ApiBenefits.SuggestEnrollmentsRequest { Filter = new ApiBenefits.SuggestEnrollmentsRequest.Types.Filter { Region = record.CorrelationId, Query = record.Message }, OrderBy = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitQueryService/FindOrgUnits and hands what it keeps to the sink of its kind.</summary>
internal static class RelayOrgUnitsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient(channel);
        var found = await source.FindOrgUnitsAsync(new ApiEmployees.FindOrgUnitsRequest() { OrderBy = "primary-relay-org-units-status" }, cancellationToken: token);
        var record = new RelayOrgUnitsStatusRecord
        {
            Status = found.Result.Status,
            Message = found.Result.Message,
            CorrelationId = found.Result.CorrelationId,
            NextPageToken = found.PageInfo.NextPageToken,
        };
        await ActOnRelayOrgUnitsStatusAsync("search-index-query", record, channel, token);
    }

    private static async Task ActOnRelayOrgUnitsStatusAsync(string kind, RelayOrgUnitsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["search-index-query"] = () => new RelayOrgUnitsStatusSearchIndexQueryWriter(new ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient(channel)).SendAsync(record, token),
            ["bank-account-query"] = () => new RelayOrgUnitsStatusBankAccountQuerySink(new ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient(channel)).SendAsync(record, token),
            ["enrollment-query"] = () => new RelayOrgUnitsStatusEnrollmentQueryRelay(new ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient(channel)).SendAsync(record, token),
        };
        await actions[kind]();
    }
}
