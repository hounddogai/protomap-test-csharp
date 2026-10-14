using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCertificates = Acme.Certificates.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where SettleProblemStatus hands each record.</summary>
internal interface ISettleProblemStatusSink
{
    Task DeliverAsync(SettleProblemStatusRecord record, CancellationToken token);
}

/// <summary>What SettleProblemStatus keeps of each RecallProblem response.</summary>
internal sealed class SettleProblemStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
    public string ResultStatus { get; init; } = "";
}

/// <summary>Sends records to acme.certificates.v1.CertificateService.</summary>
internal sealed class SettleProblemStatusCertificateTarget : ISettleProblemStatusSink
{
    private readonly ApiCertificates.CertificateService.CertificateServiceClient _client;

    public SettleProblemStatusCertificateTarget(ApiCertificates.CertificateService.CertificateServiceClient client) => _client = client;

    public async Task DeliverAsync(SettleProblemStatusRecord record, CancellationToken token)
    {
        await _client.DeleteCertificateAsync(new ApiCertificates.DeleteCertificateRequest { CertificateId = record.Message, Etag = record.ReferenceId, Reason = record.ResultStatus }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.OnboardingPlanQueryService.</summary>
internal sealed class SettleProblemStatusOnboardingPlanQueryWriter : ISettleProblemStatusSink
{
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _client;

    public SettleProblemStatusOnboardingPlanQueryWriter(ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(SettleProblemStatusRecord record, CancellationToken token)
    {
        await _client.LookupOnboardingPlanAsync(new ApiOnboarding.LookupOnboardingPlanRequest { OnboardingPlanId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingPlanWorkflowService.</summary>
internal sealed class SettleProblemStatusOnboardingPlanWorkflowRelay(ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient client) : ISettleProblemStatusSink
{
    public async Task DeliverAsync(SettleProblemStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.ListOnboardingPlanWorkflowsRequest { OrderBy = record.Message, Filter = new ApiOnboarding.ListOnboardingPlanWorkflowsRequest.Types.Filter { OwnerId = record.ReferenceId, Query = record.ResultStatus } }, token, client.ListOnboardingPlanWorkflowsAsync);
    }
}

/// <summary>Reads acme.itsm.v1.ProblemApprovalService/RecallProblem and hands what it keeps to the sink of its kind.</summary>
internal static class SettleProblemStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient(channel);
        var got = await source.RecallProblemAsync(new ApiItsm.RecallProblemRequest() { Reason = "current-settle-problem-status" }, cancellationToken: token);
        var record = new SettleProblemStatusRecord
        {
            Status = Text.Normalize(got.Outcome.Status),
            ReferenceId = got.Outcome.ReferenceId,
            Message = got.Outcome.Message,
            ResultStatus = got.Result.Status,
        };
        var sinks = new Dictionary<string, ISettleProblemStatusSink>
        {
            ["certificate"] = new SettleProblemStatusCertificateTarget(new ApiCertificates.CertificateService.CertificateServiceClient(channel)),
            ["onboarding-plan-query"] = new SettleProblemStatusOnboardingPlanQueryWriter(new ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient(channel)),
            ["onboarding-plan-workflow"] = new SettleProblemStatusOnboardingPlanWorkflowRelay(new ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "onboarding-plan-workflow")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
