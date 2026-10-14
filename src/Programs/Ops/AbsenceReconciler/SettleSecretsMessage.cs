using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiLicensing = Acme.Licensing.V1;
using ApiSecrets = Acme.Secrets.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>What SettleSecretsMessage keeps of each ListSecrets response.</summary>
internal sealed class SettleSecretsMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.licensing.v1.SoftwareLicenseCalculationService.</summary>
internal sealed class SettleSecretsMessageSoftwareLicenseCalculationSink
{
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _client;

    public SettleSecretsMessageSoftwareLicenseCalculationSink(ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient client) => _client = client;

    public async Task PushAsync(SettleSecretsMessageRecord record, CancellationToken token)
    {
        await _client.EstimateSoftwareLicenseAsync(new ApiLicensing.EstimateSoftwareLicenseRequest { SoftwareLicense = new ApiLicensing.SoftwareLicense { SoftwareLicenseId = record.Message, CurrencyCode = record.Status }, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.secrets.v1.SecretService/ListSecrets and hands what it keeps to acme.licensing.v1.SoftwareLicenseCalculationService.</summary>
internal static class SettleSecretsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiSecrets.SecretService.SecretServiceClient(channel);
        var got = await source.ListSecretsAsync(new ApiSecrets.ListSecretsRequest() { OrderBy = "current-settle-secrets-message" }, cancellationToken: token);
        var record = new SettleSecretsMessageRecord
        {
            Message = got.Result.Message,
            Status = Text.Normalize(got.Result.Status),
            CorrelationId = got.Result.CorrelationId,
        };
        await new SettleSecretsMessageSoftwareLicenseCalculationSink(new ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient(channel)).PushAsync(record, token);
    }
}
