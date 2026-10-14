using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiLearning = Acme.Learning.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where SyncOnboardingPlanStatus hands each record.</summary>
internal interface ISyncOnboardingPlanStatusTarget
{
    Task ApplyAsync(SyncOnboardingPlanStatusRecord record, CancellationToken token);
}

/// <summary>What SyncOnboardingPlanStatus keeps of each ValidateOnboardingPlan response.</summary>
internal sealed class SyncOnboardingPlanStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.learning.v1.CertificationCalculationService.</summary>
internal sealed class SyncOnboardingPlanStatusCertificationCalculationTarget(ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient client) : ISyncOnboardingPlanStatusTarget
{
    public async Task ApplyAsync(SyncOnboardingPlanStatusRecord record, CancellationToken token)
    {
        await client.CompareCertificationOptionsAsync(new ApiLearning.CompareCertificationOptionsRequest { CertificationId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.recruiting.v1.OfferReportingService.</summary>
internal sealed class SyncOnboardingPlanStatusOfferReportingWriter : ISyncOnboardingPlanStatusTarget
{
    private readonly ApiRecruiting.OfferReportingService.OfferReportingServiceClient _client;

    public SyncOnboardingPlanStatusOfferReportingWriter(ApiRecruiting.OfferReportingService.OfferReportingServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncOnboardingPlanStatusRecord record, CancellationToken token)
    {
        await _client.FetchOfferReportAsync(new ApiRecruiting.FetchOfferReportRequest { OfferId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.JobOpeningApprovalService.</summary>
internal sealed class SyncOnboardingPlanStatusJobOpeningApprovalSink : ISyncOnboardingPlanStatusTarget
{
    private readonly ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient _client;

    public SyncOnboardingPlanStatusJobOpeningApprovalSink(ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncOnboardingPlanStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.SubmitJobOpeningRequest { Reason = record.Message, JobOpening = new ApiRecruiting.JobOpening { Number = record.ReferenceId } }, token, _client.SubmitJobOpeningAsync);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingPlanValidationService/ValidateOnboardingPlan and hands what it keeps to the sink of its kind.</summary>
internal static class SyncOnboardingPlanStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient(channel);
        var got = await source.ValidateOnboardingPlanAsync(new ApiOnboarding.ValidateOnboardingPlanRequest() { Reason = "current-sync-onboarding-plan-status" }, cancellationToken: token);
        var record = new SyncOnboardingPlanStatusRecord
        {
            Status = Text.Normalize(got.Outcome.Status),
            ReferenceId = Text.Normalize(got.Outcome.ReferenceId),
            Message = got.Result.Message,
        };
        var sinks = new ISyncOnboardingPlanStatusTarget[]
        {
            new SyncOnboardingPlanStatusCertificationCalculationTarget(new ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient(channel)),
            new SyncOnboardingPlanStatusOfferReportingWriter(new ApiRecruiting.OfferReportingService.OfferReportingServiceClient(channel)),
            new SyncOnboardingPlanStatusJobOpeningApprovalSink(new ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient(channel)),
        };
        await sinks[0].ApplyAsync(record, token);
    }
}
