using Grpc.Net.Client;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ExportCertificationsId hands each record.</summary>
internal interface IExportCertificationsIdWriter
{
    Task WriteAsync(ExportCertificationsIdRecord record, CancellationToken token);
}

/// <summary>What ExportCertificationsId keeps of each LintCertifications response.</summary>
internal sealed class ExportCertificationsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Sends records to acme.corporateevents.v1.VenueLifecycleService.</summary>
internal sealed class ExportCertificationsIdVenueLifecycleWriter(ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient client) : IExportCertificationsIdWriter
{
    public async Task WriteAsync(ExportCertificationsIdRecord record, CancellationToken token)
    {
        await client.RenewVenueAsync(new ApiCorporateevents.RenewVenueRequest { VenueId = record.Notes, Reason = record.ReferenceId, Details = new ApiCorporateevents.RenewVenueRequest.Types.Details { ReasonCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.corporateevents.v1.RegistrationQueryService.</summary>
internal sealed class ExportCertificationsIdRegistrationQuerySink(ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient client) : IExportCertificationsIdWriter
{
    public async Task WriteAsync(ExportCertificationsIdRecord record, CancellationToken token)
    {
        await client.LookupRegistrationAsync(new ApiCorporateevents.LookupRegistrationRequest { RegistrationId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.OfferLifecycleService.</summary>
internal sealed class ExportCertificationsIdOfferLifecycleRelay : IExportCertificationsIdWriter
{
    private readonly ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient _client;

    public ExportCertificationsIdOfferLifecycleRelay(ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient client) => _client = client;

    public async Task WriteAsync(ExportCertificationsIdRecord record, CancellationToken token)
    {
        await _client.GetOfferLifecycleAsync(new ApiRecruiting.GetOfferLifecycleRequest { OfferId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.CertificationValidationService/LintCertifications and hands what it keeps to the sink of its kind.</summary>
internal static class ExportCertificationsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CertificationValidationService.CertificationValidationServiceClient(channel);
        var reply = await source.LintCertificationsAsync(new ApiLearning.LintCertificationsRequest() { CertificationId = "main-export-certifications-id" }, cancellationToken: token);
        var record = new ExportCertificationsIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Notes = reply.Certification.Notes,
            Status = reply.Result.Status,
            ReferenceId = reply.Outcome.ReferenceId,
        };
        var sinks = new Dictionary<string, IExportCertificationsIdWriter>
        {
            ["venue-lifecycle"] = new ExportCertificationsIdVenueLifecycleWriter(new ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient(channel)),
            ["registration-query"] = new ExportCertificationsIdRegistrationQuerySink(new ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient(channel)),
            ["offer-lifecycle"] = new ExportCertificationsIdOfferLifecycleRelay(new ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("EXPORT_CERTIFICATIONS_ID_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].WriteAsync(record, token);
    }
}
