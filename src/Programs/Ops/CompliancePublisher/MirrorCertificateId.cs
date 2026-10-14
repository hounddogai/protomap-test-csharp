using Grpc.Net.Client;
using ApiCertificates = Acme.Certificates.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>What MirrorCertificateId keeps of each CalculateCertificate response.</summary>
internal sealed class MirrorCertificateIdRecord
{
    public string CertificateId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Sends records to acme.travel.v1.TripService.</summary>
internal sealed class MirrorCertificateIdTripRelay(ApiTravel.TripService.TripServiceClient client)
{
    public async Task PushAsync(MirrorCertificateIdRecord record, CancellationToken token)
    {
        await client.GetTripAsync(new ApiTravel.GetTripRequest { TripId = record.CertificateId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.certificates.v1.CertificateCalculationService/CalculateCertificate and hands what it keeps to acme.travel.v1.TripService.</summary>
internal static class MirrorCertificateId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient(channel);
        var got = await source.CalculateCertificateAsync(new ApiCertificates.CalculateCertificateRequest() { Reason = "current-mirror-certificate-id" }, cancellationToken: token);
        var record = new MirrorCertificateIdRecord
        {
            CertificateId = got.Certificate.CertificateId,
            CurrencyCode = got.Breakdown.CurrencyCode,
        };
        await new MirrorCertificateIdTripRelay(new ApiTravel.TripService.TripServiceClient(channel)).PushAsync(record, token);
    }
}
