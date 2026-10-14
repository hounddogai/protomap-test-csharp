using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAudit = Acme.Audit.V1;
using ApiCertificates = Acme.Certificates.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where BackfillAuditEventMessage hands each record.</summary>
internal interface IBackfillAuditEventMessageWriter
{
    Task SendAsync(BackfillAuditEventMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillAuditEventMessage keeps of each DeleteAuditEvent response.</summary>
internal sealed class BackfillAuditEventMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.certificates.v1.CertificateCalculationService.</summary>
internal sealed class BackfillAuditEventMessageCertificateCalculationWriter(ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient client) : IBackfillAuditEventMessageWriter
{
    public async Task SendAsync(BackfillAuditEventMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCertificates.SimulateCertificateRequest { Details = new ApiCertificates.SimulateCertificateRequest.Types.Details { CurrencyCode = record.Message }, Certificate = new ApiCertificates.Certificate { Notes = record.CorrelationId, CertificateId = record.Status } }, token, client.SimulateCertificateAsync);
    }
}

/// <summary>Passes records to acme.sellers.v1.SellerService.</summary>
internal sealed class BackfillAuditEventMessageSellerRelay : IBackfillAuditEventMessageWriter
{
    private readonly ApiSellers.SellerService.SellerServiceClient _client;

    public BackfillAuditEventMessageSellerRelay(ApiSellers.SellerService.SellerServiceClient client) => _client = client;

    public async Task SendAsync(BackfillAuditEventMessageRecord record, CancellationToken token)
    {
        await _client.ListSellersAsync(new ApiSellers.ListSellersRequest { Filter = new ApiSellers.ListSellersRequest.Types.Filter { Query = record.Message }, OrderBy = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.configuration.v1.ConfigSetCalculationService.</summary>
internal sealed class BackfillAuditEventMessageConfigSetCalculationSink : IBackfillAuditEventMessageWriter
{
    private readonly ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient _client;

    public BackfillAuditEventMessageConfigSetCalculationSink(ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient client) => _client = client;

    public async Task SendAsync(BackfillAuditEventMessageRecord record, CancellationToken token)
    {
        await _client.CalculateConfigSetAsync(new ApiConfiguration.CalculateConfigSetRequest { ConfigSet = new ApiConfiguration.ConfigSet { Name = record.Message, ParentId = record.CorrelationId }, Details = new ApiConfiguration.CalculateConfigSetRequest.Types.Details { CurrencyCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.audit.v1.AuditEventService/DeleteAuditEvent and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillAuditEventMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudit.AuditEventService.AuditEventServiceClient(channel);
        var got = await source.DeleteAuditEventAsync(new ApiAudit.DeleteAuditEventRequest() { Reason = "current-backfill-audit-event-message" }, cancellationToken: token);
        var record = new BackfillAuditEventMessageRecord
        {
            Message = got.Result.Message,
            Status = got.Result.Status,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IBackfillAuditEventMessageWriter>
        {
            ["certificate-calculation"] = new BackfillAuditEventMessageCertificateCalculationWriter(new ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient(channel)),
            ["seller"] = new BackfillAuditEventMessageSellerRelay(new ApiSellers.SellerService.SellerServiceClient(channel)),
            ["config-set-calculation"] = new BackfillAuditEventMessageConfigSetCalculationSink(new ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "certificate-calculation")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
