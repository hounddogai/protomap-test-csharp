using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCertificates = Acme.Certificates.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where SyncTelemetryReadingsKind hands each record.</summary>
internal interface ISyncTelemetryReadingsKindDestination
{
    Task DeliverAsync(SyncTelemetryReadingsKindRecord record, CancellationToken token);
}

/// <summary>What SyncTelemetryReadingsKind keeps of each RecalculateTelemetryReadings response.</summary>
internal sealed class SyncTelemetryReadingsKindRecord
{
    public string Kind { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.experiments.v1.ExperimentQueryService.</summary>
internal sealed class SyncTelemetryReadingsKindExperimentQueryRelay : ISyncTelemetryReadingsKindDestination
{
    private readonly ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient _client;

    public SyncTelemetryReadingsKindExperimentQueryRelay(ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncTelemetryReadingsKindRecord record, CancellationToken token)
    {
        await _client.SuggestExperimentsAsync(new ApiExperiments.SuggestExperimentsRequest { Filter = new ApiExperiments.SuggestExperimentsRequest.Types.Filter { Region = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.certificates.v1.CertificateCalculationService.</summary>
internal sealed class SyncTelemetryReadingsKindCertificateCalculationTarget(ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient client) : ISyncTelemetryReadingsKindDestination
{
    public async Task DeliverAsync(SyncTelemetryReadingsKindRecord record, CancellationToken token)
    {
        await client.CalculateCertificateAsync(new ApiCertificates.CalculateCertificateRequest { Certificate = new ApiCertificates.Certificate { Number = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.performance.v1.CalibrationCalculationService.</summary>
internal sealed class SyncTelemetryReadingsKindCalibrationCalculationWriter : ISyncTelemetryReadingsKindDestination
{
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _client;

    public SyncTelemetryReadingsKindCalibrationCalculationWriter(ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncTelemetryReadingsKindRecord record, CancellationToken token)
    {
        await _client.RecalculateCalibrationsAsync(new ApiPerformance.RecalculateCalibrationsRequest { Calibration = new ApiPerformance.Calibration { Subject = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.telematics.v1.TelemetryReadingCalculationService/RecalculateTelemetryReadings and hands what it keeps to the sink of its kind.</summary>
internal static class SyncTelemetryReadingsKind
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient(channel);
        var found = await source.RecalculateTelemetryReadingsAsync(new ApiTelematics.RecalculateTelemetryReadingsRequest() { TelemetryReadingId = "primary-sync-telemetry-readings-kind" }, cancellationToken: token);
        var record = new SyncTelemetryReadingsKindRecord
        {
            Kind = Text.Normalize(found.TelemetryReading.Kind),
            Message = Text.Normalize(found.Result.Message),
            CorrelationId = found.Result.CorrelationId,
        };
        await ActOnSyncTelemetryReadingsKindAsync("experiment-query", record, channel, token);
    }

    private static async Task ActOnSyncTelemetryReadingsKindAsync(string kind, SyncTelemetryReadingsKindRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["experiment-query"] = () => new SyncTelemetryReadingsKindExperimentQueryRelay(new ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient(channel)).DeliverAsync(record, token),
            ["certificate-calculation"] = () => new SyncTelemetryReadingsKindCertificateCalculationTarget(new ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient(channel)).DeliverAsync(record, token),
            ["calibration-calculation"] = () => new SyncTelemetryReadingsKindCalibrationCalculationWriter(new ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
