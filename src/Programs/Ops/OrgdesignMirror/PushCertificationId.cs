using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAudit = Acme.Audit.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where PushCertificationId hands each record.</summary>
internal interface IPushCertificationIdTarget
{
    Task ForwardAsync(PushCertificationIdRecord record, CancellationToken token);
}

/// <summary>What PushCertificationId keeps of each RecallCertification response.</summary>
internal sealed class PushCertificationIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string CertificationId { get; init; } = "";
}

/// <summary>Passes records to acme.audit.v1.AuditEventQueryService.</summary>
internal sealed class PushCertificationIdAuditEventQueryRelay : IPushCertificationIdTarget
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _client;

    public PushCertificationIdAuditEventQueryRelay(ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(PushCertificationIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAudit.SuggestAuditEventsRequest { Filter = new ApiAudit.SuggestAuditEventsRequest.Types.Filter { Query = record.CorrelationId } }, token, _client.SuggestAuditEventsAsync);
    }
}

/// <summary>Passes records to acme.keymanagement.v1.CryptoKeyCalculationService.</summary>
internal sealed class PushCertificationIdCryptoKeyCalculationRelay(ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient client) : IPushCertificationIdTarget
{
    public async Task ForwardAsync(PushCertificationIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiKeymanagement.RecalculateCryptoKeysRequest { CryptoKey = new ApiKeymanagement.CryptoKey { CryptoKeyId = record.CertificationId, CurrencyCode = record.Status }, CryptoKeyId = record.CorrelationId }, token, client.RecalculateCryptoKeysAsync);
    }
}

/// <summary>Reads acme.learning.v1.CertificationApprovalService/RecallCertification and hands what it keeps to the sink of its kind.</summary>
internal static class PushCertificationId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient(channel);
        var found = await source.RecallCertificationAsync(new ApiLearning.RecallCertificationRequest() { CertificationId = "main-push-certification-id" }, cancellationToken: token);
        var record = new PushCertificationIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Status = Text.Normalize(found.Outcome.Status),
            CertificationId = found.Certification.CertificationId,
        };
        await ActOnPushCertificationIdAsync("crypto-key-calculation", record, channel, token);
    }

    private static async Task ActOnPushCertificationIdAsync(string kind, PushCertificationIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["audit-event-query"] = () => new PushCertificationIdAuditEventQueryRelay(new ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient(channel)).ForwardAsync(record, token),
            ["crypto-key-calculation"] = () => new PushCertificationIdCryptoKeyCalculationRelay(new ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
