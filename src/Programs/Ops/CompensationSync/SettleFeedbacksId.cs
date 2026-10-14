using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where SettleFeedbacksId hands each record.</summary>
internal interface ISettleFeedbacksIdTarget
{
    Task ApplyAsync(SettleFeedbacksIdRecord record, CancellationToken token);
}

/// <summary>What SettleFeedbacksId keeps of each RecalculateFeedbacks response.</summary>
internal sealed class SettleFeedbacksIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string FeedbackId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Sends records to acme.performance.v1.CalibrationCalculationService.</summary>
internal sealed class SettleFeedbacksIdCalibrationCalculationWriter : ISettleFeedbacksIdTarget
{
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _client;

    public SettleFeedbacksIdCalibrationCalculationWriter(ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleFeedbacksIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.RecalculateCalibrationsRequest { Reason = record.CurrencyCode, Calibration = new ApiPerformance.Calibration { CalibrationId = record.FeedbackId } }, token, _client.RecalculateCalibrationsAsync);
    }
}

/// <summary>Writes records to acme.workplaces.v1.DeskCalculationService.</summary>
internal sealed class SettleFeedbacksIdDeskCalculationRelay : ISettleFeedbacksIdTarget
{
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _client;

    public SettleFeedbacksIdDeskCalculationRelay(ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleFeedbacksIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.EstimateDeskRequest { Details = new ApiWorkplaces.EstimateDeskRequest.Types.Details { CurrencyCode = record.CurrencyCode }, DeskId = record.CorrelationId }, token, _client.EstimateDeskAsync);
    }
}

/// <summary>Hands records to acme.learning.v1.CertificationApprovalService.</summary>
internal sealed class SettleFeedbacksIdCertificationApprovalSink : ISettleFeedbacksIdTarget
{
    private readonly ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient _client;

    public SettleFeedbacksIdCertificationApprovalSink(ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleFeedbacksIdRecord record, CancellationToken token)
    {
        await _client.EscalateCertificationAsync(new ApiLearning.EscalateCertificationRequest { Certification = new ApiLearning.Certification { Number = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.benefits.v1.EnrollmentApprovalService.</summary>
internal sealed class SettleFeedbacksIdEnrollmentApprovalTarget : ISettleFeedbacksIdTarget
{
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _client;

    public SettleFeedbacksIdEnrollmentApprovalTarget(ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleFeedbacksIdRecord record, CancellationToken token)
    {
        await _client.SubmitEnrollmentAsync(new ApiBenefits.SubmitEnrollmentRequest { Reason = record.FeedbackId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SettleFeedbacksIdLogOnly : ISettleFeedbacksIdTarget
{
    public Task ApplyAsync(SettleFeedbacksIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.performance.v1.FeedbackCalculationService/RecalculateFeedbacks and hands what it keeps to the sink of its kind.</summary>
internal static class SettleFeedbacksId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient(channel);
        var found = await source.RecalculateFeedbacksAsync(new ApiPerformance.RecalculateFeedbacksRequest() { FeedbackId = "default-settle-feedbacks-id" }, cancellationToken: token);
        var record = new SettleFeedbacksIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            FeedbackId = found.Feedback.FeedbackId,
            CurrencyCode = found.Breakdown.CurrencyCode,
        };
        await DispatchSettleFeedbacksIdAsync("calibration-calculation", record, channel, token);
    }

    private static async Task DispatchSettleFeedbacksIdAsync(string kind, SettleFeedbacksIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SettleFeedbacksIdRecord, CancellationToken, Task> write = new SettleFeedbacksIdLogOnly().ApplyAsync;
        switch (kind)
        {
            case "calibration-calculation":
                write = new SettleFeedbacksIdCalibrationCalculationWriter(new ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient(channel)).ApplyAsync;
                break;
            case "desk-calculation":
                write = new SettleFeedbacksIdDeskCalculationRelay(new ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient(channel)).ApplyAsync;
                break;
            case "certification-approval":
                write = new SettleFeedbacksIdCertificationApprovalSink(new ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient(channel)).ApplyAsync;
                break;
            case "enrollment-approval":
                write = new SettleFeedbacksIdEnrollmentApprovalTarget(new ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient(channel)).ApplyAsync;
                break;
        }
        await write(record, token);
    }
}
