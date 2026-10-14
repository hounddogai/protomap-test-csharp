using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBanking = Acme.Banking.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiEmployees = Acme.Employees.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where CopyBankAccountsId hands each record.</summary>
internal interface ICopyBankAccountsIdSink
{
    Task PushAsync(CopyBankAccountsIdRecord record, CancellationToken token);
}

/// <summary>What CopyBankAccountsId keeps of each BrowseBankAccounts response.</summary>
internal sealed class CopyBankAccountsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.benefits.v1.DependentApprovalService.</summary>
internal sealed class CopyBankAccountsIdDependentApprovalRelay : ICopyBankAccountsIdSink
{
    private readonly ApiBenefits.DependentApprovalService.DependentApprovalServiceClient _client;

    public CopyBankAccountsIdDependentApprovalRelay(ApiBenefits.DependentApprovalService.DependentApprovalServiceClient client) => _client = client;

    public async Task PushAsync(CopyBankAccountsIdRecord record, CancellationToken token)
    {
        await _client.SubmitDependentAsync(new ApiBenefits.SubmitDependentRequest { DependentId = record.CorrelationId, Reason = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.employees.v1.EmploymentContractApprovalService.</summary>
internal sealed class CopyBankAccountsIdEmploymentContractApprovalSink(ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient client) : ICopyBankAccountsIdSink
{
    public async Task PushAsync(CopyBankAccountsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.SubmitEmploymentContractRequest { EmploymentContract = new ApiEmployees.EmploymentContract { Number = record.CorrelationId }, Details = new ApiEmployees.SubmitEmploymentContractRequest.Types.Details { ApproverEmail = record.NextPageToken } }, token, client.SubmitEmploymentContractAsync);
    }
}

/// <summary>Reads acme.banking.v1.BankAccountQueryService/BrowseBankAccounts and hands what it keeps to the sink of its kind.</summary>
internal static class CopyBankAccountsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient(channel);
        var found = await source.BrowseBankAccountsAsync(new ApiBanking.BrowseBankAccountsRequest() { OrderBy = "default-copy-bank-accounts-id" }, cancellationToken: token);
        var record = new CopyBankAccountsIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            NextPageToken = found.PageInfo.NextPageToken,
            Status = found.Result.Status,
        };
        var sinks = new List<ICopyBankAccountsIdSink>
        {
            new CopyBankAccountsIdDependentApprovalRelay(new ApiBenefits.DependentApprovalService.DependentApprovalServiceClient(channel)),
            new CopyBankAccountsIdEmploymentContractApprovalSink(new ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.PushAsync(record, token);
    }
}
