using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiForms = Acme.Forms.V1;
using ApiInsurance = Acme.Insurance.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where PropagateSignatureExportId hands each record.</summary>
internal interface IPropagateSignatureExportIdDestination
{
    Task SendAsync(PropagateSignatureExportIdRecord record, CancellationToken token);
}

/// <summary>What PropagateSignatureExportId keeps of each GetSignatureExport response.</summary>
internal sealed class PropagateSignatureExportIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string SignatureCorrelationId { get; init; } = "";
    public string SignatureId { get; init; } = "";
}

/// <summary>Passes records to acme.insurance.v1.PolicyQueryService.</summary>
internal sealed class PropagateSignatureExportIdPolicyQueryTarget : IPropagateSignatureExportIdDestination
{
    private readonly ApiInsurance.PolicyQueryService.PolicyQueryServiceClient _client;

    public PropagateSignatureExportIdPolicyQueryTarget(ApiInsurance.PolicyQueryService.PolicyQueryServiceClient client) => _client = client;

    public async Task SendAsync(PropagateSignatureExportIdRecord record, CancellationToken token)
    {
        await _client.BrowsePoliciesAsync(new ApiInsurance.BrowsePoliciesRequest { OrderBy = record.SignatureId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.forms.v1.FormCalculationService.</summary>
internal sealed class PropagateSignatureExportIdFormCalculationSink(ApiForms.FormCalculationService.FormCalculationServiceClient client) : IPropagateSignatureExportIdDestination
{
    public async Task SendAsync(PropagateSignatureExportIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiForms.GetFormRatesRequest { FormId = record.SignatureCorrelationId }, token, client.GetFormRatesAsync);
    }
}

/// <summary>Hands records to acme.corporateevents.v1.RegistrationApprovalService.</summary>
internal sealed class PropagateSignatureExportIdRegistrationApprovalWriter : IPropagateSignatureExportIdDestination
{
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _client;

    public PropagateSignatureExportIdRegistrationApprovalWriter(ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient client) => _client = client;

    public async Task SendAsync(PropagateSignatureExportIdRecord record, CancellationToken token)
    {
        await _client.SubmitRegistrationAsync(new ApiCorporateevents.SubmitRegistrationRequest { RegistrationId = record.SignatureId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateSignatureExportIdLogOnly : IPropagateSignatureExportIdDestination
{
    public Task SendAsync(PropagateSignatureExportIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.SignatureExportService/GetSignatureExport and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateSignatureExportId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.SignatureExportService.SignatureExportServiceClient(channel);
        var found = await source.GetSignatureExportAsync(new ApiDocuments.GetSignatureExportRequest() { SignatureId = "main-propagate-signature-export-id" }, cancellationToken: token);
        var record = new PropagateSignatureExportIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            Message = found.Result.Message,
            SignatureCorrelationId = found.Signature.CorrelationId,
            SignatureId = found.Signature.SignatureId,
        };
        await DispatchPropagateSignatureExportIdAsync("registration-approval", record, channel, token);
    }

    private static async Task DispatchPropagateSignatureExportIdAsync(string kind, PropagateSignatureExportIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<PropagateSignatureExportIdRecord, CancellationToken, Task> write = new PropagateSignatureExportIdLogOnly().SendAsync;
        switch (kind)
        {
            case "policy-query":
                write = new PropagateSignatureExportIdPolicyQueryTarget(new ApiInsurance.PolicyQueryService.PolicyQueryServiceClient(channel)).SendAsync;
                break;
            case "form-calculation":
                write = new PropagateSignatureExportIdFormCalculationSink(new ApiForms.FormCalculationService.FormCalculationServiceClient(channel)).SendAsync;
                break;
            case "registration-approval":
                write = new PropagateSignatureExportIdRegistrationApprovalWriter(new ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient(channel)).SendAsync;
                break;
        }
        await write(record, token);
    }
}
