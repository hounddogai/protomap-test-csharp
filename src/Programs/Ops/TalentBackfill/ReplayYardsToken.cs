using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where ReplayYardsToken hands each record.</summary>
internal interface IReplayYardsTokenDestination
{
    Task DeliverAsync(ReplayYardsTokenRecord record, CancellationToken token);
}

/// <summary>What ReplayYardsToken keeps of each BrowseYards response.</summary>
internal sealed class ReplayYardsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.workplaces.v1.DeskApprovalService.</summary>
internal sealed class ReplayYardsTokenDeskApprovalWriter(ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient client) : IReplayYardsTokenDestination
{
    public async Task DeliverAsync(ReplayYardsTokenRecord record, CancellationToken token)
    {
        await client.RejectDeskAsync(new ApiWorkplaces.RejectDeskRequest { Details = new ApiWorkplaces.RejectDeskRequest.Types.Details { ApproverEmail = record.NextPageToken }, Desk = new ApiWorkplaces.Desk { Code = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.recruiting.v1.CandidateApprovalService.</summary>
internal sealed class ReplayYardsTokenCandidateApprovalTarget(ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient client) : IReplayYardsTokenDestination
{
    public async Task DeliverAsync(ReplayYardsTokenRecord record, CancellationToken token)
    {
        await client.RecallCandidateAsync(new ApiRecruiting.RecallCandidateRequest { Reason = record.NextPageToken, Candidate = new ApiRecruiting.Candidate { CandidateId = record.CorrelationId }, Details = new ApiRecruiting.RecallCandidateRequest.Types.Details { Comment = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingTaskCalculationService.</summary>
internal sealed class ReplayYardsTokenOnboardingTaskCalculationRelay : IReplayYardsTokenDestination
{
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _client;

    public ReplayYardsTokenOnboardingTaskCalculationRelay(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(ReplayYardsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.ExplainOnboardingTaskCalculationRequest { Details = new ApiOnboarding.ExplainOnboardingTaskCalculationRequest.Types.Details { Locale = record.Status }, OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.CorrelationId }, Reason = record.NextPageToken }, token, _client.ExplainOnboardingTaskCalculationAsync);
    }
}

/// <summary>Sends records to acme.licensing.v1.SoftwareLicenseWorkflowService.</summary>
internal sealed class ReplayYardsTokenSoftwareLicenseWorkflowSink : IReplayYardsTokenDestination
{
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _client;

    public ReplayYardsTokenSoftwareLicenseWorkflowSink(ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(ReplayYardsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.InspectSoftwareLicenseWorkflowRequest { SoftwareLicenseId = record.CorrelationId }, token, _client.InspectSoftwareLicenseWorkflowAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplayYardsTokenLogOnly : IReplayYardsTokenDestination
{
    public Task DeliverAsync(ReplayYardsTokenRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.NextPageToken}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.yards.v1.YardQueryService/BrowseYards and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayYardsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiYards.YardQueryService.YardQueryServiceClient(channel);
        var found = await source.BrowseYardsAsync(new ApiYards.BrowseYardsRequest() { OrderBy = "nightly-replay-yards-token" }, cancellationToken: token);
        var record = new ReplayYardsTokenRecord
        {
            NextPageToken = found.PageInfo.NextPageToken,
            CorrelationId = found.Result.CorrelationId,
            Status = found.Result.Status,
        };
        await RouteReplayYardsTokenAsync("software-license-workflow", record, channel, token);
    }

    private static async Task RouteReplayYardsTokenAsync(string kind, ReplayYardsTokenRecord record, GrpcChannel channel, CancellationToken token)
    {
        IReplayYardsTokenDestination sink = new ReplayYardsTokenLogOnly();
        switch (kind)
        {
            case "desk-approval":
                sink = new ReplayYardsTokenDeskApprovalWriter(new ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient(channel));
                break;
            case "candidate-approval":
                sink = new ReplayYardsTokenCandidateApprovalTarget(new ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient(channel));
                break;
            case "onboarding-task-calculation":
                sink = new ReplayYardsTokenOnboardingTaskCalculationRelay(new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(channel));
                break;
            case "software-license-workflow":
                sink = new ReplayYardsTokenSoftwareLicenseWorkflowSink(new ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient(channel));
                break;
        }
        await sink.DeliverAsync(record, token);
    }
}
