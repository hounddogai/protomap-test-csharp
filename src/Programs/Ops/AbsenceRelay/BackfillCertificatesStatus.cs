using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCertificates = Acme.Certificates.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>What BackfillCertificatesStatus keeps of each SuggestCertificates response.</summary>
internal sealed class BackfillCertificatesStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.ratelimiting.v1.RateLimitCalculationService.</summary>
internal sealed class BackfillCertificatesStatusRateLimitCalculationRelay
{
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _client;

    public BackfillCertificatesStatusRateLimitCalculationRelay(ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillCertificatesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRatelimiting.SimulateRateLimitRequest { Details = new ApiRatelimiting.SimulateRateLimitRequest.Types.Details { CurrencyCode = record.NextPageToken }, RateLimitId = record.CorrelationId, RateLimit = new ApiRatelimiting.RateLimit { Name = record.Status } }, token, _client.SimulateRateLimitAsync);
    }
}

/// <summary>Reads acme.certificates.v1.CertificateQueryService/SuggestCertificates and hands what it keeps to acme.ratelimiting.v1.RateLimitCalculationService.</summary>
internal static class BackfillCertificatesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCertificates.CertificateQueryService.CertificateQueryServiceClient(channel);
        var reply = await source.SuggestCertificatesAsync(new ApiCertificates.SuggestCertificatesRequest() { OrderBy = "main-backfill-certificates-status" }, cancellationToken: token);
        var record = new BackfillCertificatesStatusRecord
        {
            Status = reply.Result.Status,
            NextPageToken = reply.PageInfo.NextPageToken,
            CorrelationId = reply.Result.CorrelationId,
        };
        await new BackfillCertificatesStatusRateLimitCalculationRelay(new ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient(channel)).ApplyAsync(record, token);
    }
}
