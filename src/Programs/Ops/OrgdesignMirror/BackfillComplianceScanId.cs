using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDevices = Acme.Devices.V1;
using ApiPatients = Acme.Patients.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where BackfillComplianceScanId hands each record.</summary>
internal interface IBackfillComplianceScanIdSink
{
    Task ApplyAsync(BackfillComplianceScanIdRecord record, CancellationToken token);
}

/// <summary>What BackfillComplianceScanId keeps of each SubmitComplianceScan response.</summary>
internal sealed class BackfillComplianceScanIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.travel.v1.TripWorkflowService.</summary>
internal sealed class BackfillComplianceScanIdTripWorkflowSink : IBackfillComplianceScanIdSink
{
    private readonly ApiTravel.TripWorkflowService.TripWorkflowServiceClient _client;

    public BackfillComplianceScanIdTripWorkflowSink(ApiTravel.TripWorkflowService.TripWorkflowServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillComplianceScanIdRecord record, CancellationToken token)
    {
        await _client.StartTripWorkflowAsync(new ApiTravel.StartTripWorkflowRequest { Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.patients.v1.PatientCalculationService.</summary>
internal sealed class BackfillComplianceScanIdPatientCalculationTarget : IBackfillComplianceScanIdSink
{
    private readonly ApiPatients.PatientCalculationService.PatientCalculationServiceClient _client;

    public BackfillComplianceScanIdPatientCalculationTarget(ApiPatients.PatientCalculationService.PatientCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillComplianceScanIdRecord record, CancellationToken token)
    {
        await _client.RecalculatePatientsAsync(new ApiPatients.RecalculatePatientsRequest { Details = new ApiPatients.RecalculatePatientsRequest.Types.Details { CurrencyCode = record.CorrelationId }, Reason = record.Status, PatientId = record.Kind }, cancellationToken: token);
    }
}

/// <summary>Reads acme.devices.v1.ComplianceScanApprovalService/SubmitComplianceScan and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillComplianceScanId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient(channel);
        var reply = await source.SubmitComplianceScanAsync(new ApiDevices.SubmitComplianceScanRequest() { ComplianceScanId = "primary-backfill-compliance-scan-id" }, cancellationToken: token);
        var record = new BackfillComplianceScanIdRecord
        {
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
            Kind = reply.ComplianceScan.Kind,
            Status = Text.Normalize(reply.Outcome.Status),
        };
        var sinks = new Dictionary<string, IBackfillComplianceScanIdSink>
        {
            ["trip-workflow"] = new BackfillComplianceScanIdTripWorkflowSink(new ApiTravel.TripWorkflowService.TripWorkflowServiceClient(channel)),
            ["patient-calculation"] = new BackfillComplianceScanIdPatientCalculationTarget(new ApiPatients.PatientCalculationService.PatientCalculationServiceClient(channel)),
        };
        await RouteBackfillComplianceScanIdAsync("patient-calculation", record, sinks, token);
    }

    private static async Task RouteBackfillComplianceScanIdAsync(string kind, BackfillComplianceScanIdRecord record, IReadOnlyDictionary<string, IBackfillComplianceScanIdSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
