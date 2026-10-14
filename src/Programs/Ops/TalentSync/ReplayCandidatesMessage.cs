using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCertificates = Acme.Certificates.V1;
using ApiCommon = Acme.Common.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where ReplayCandidatesMessage hands each record.</summary>
internal interface IReplayCandidatesMessageTarget
{
    Task ApplyAsync(ReplayCandidatesMessageRecord record, CancellationToken token);
}

/// <summary>What ReplayCandidatesMessage keeps of each LintCandidates response.</summary>
internal sealed class ReplayCandidatesMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string CandidateId { get; init; } = "";
}

/// <summary>Sends records to acme.licensing.v1.SoftwareLicenseService.</summary>
internal sealed class ReplayCandidatesMessageSoftwareLicenseTarget : IReplayCandidatesMessageTarget
{
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _client;

    public ReplayCandidatesMessageSoftwareLicenseTarget(ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient client) => _client = client;

    public async Task ApplyAsync(ReplayCandidatesMessageRecord record, CancellationToken token)
    {
        await _client.ListSoftwareLicensesAsync(new ApiLicensing.ListSoftwareLicensesRequest { Page = new ApiCommon.PageRequest { PageToken = record.CandidateId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.certificates.v1.CertificateService.</summary>
internal sealed class ReplayCandidatesMessageCertificateTarget : IReplayCandidatesMessageTarget
{
    private readonly ApiCertificates.CertificateService.CertificateServiceClient _client;

    public ReplayCandidatesMessageCertificateTarget(ApiCertificates.CertificateService.CertificateServiceClient client) => _client = client;

    public async Task ApplyAsync(ReplayCandidatesMessageRecord record, CancellationToken token)
    {
        await _client.GetCertificateAsync(new ApiCertificates.GetCertificateRequest { CertificateId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.scheduling.v1.SwapRequestLifecycleService.</summary>
internal sealed class ReplayCandidatesMessageSwapRequestLifecycleWriter(ApiScheduling.SwapRequestLifecycleService.SwapRequestLifecycleServiceClient client) : IReplayCandidatesMessageTarget
{
    public async Task ApplyAsync(ReplayCandidatesMessageRecord record, CancellationToken token)
    {
        await client.ScheduleSwapRequestExpiryAsync(new ApiScheduling.ScheduleSwapRequestExpiryRequest { SwapRequest = new ApiScheduling.SwapRequest { Subject = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.recruiting.v1.CandidateValidationService/LintCandidates and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayCandidatesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient(channel);
        var got = await source.LintCandidatesAsync(new ApiRecruiting.LintCandidatesRequest() { Reason = "current-replay-candidates-message" }, cancellationToken: token);
        var record = new ReplayCandidatesMessageRecord
        {
            Message = got.Result.Message,
            Status = got.Result.Status,
            CandidateId = Text.Normalize(got.Candidate.CandidateId),
        };
        var sinks = new Dictionary<string, IReplayCandidatesMessageTarget>
        {
            ["software-license"] = new ReplayCandidatesMessageSoftwareLicenseTarget(new ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient(channel)),
            ["certificate"] = new ReplayCandidatesMessageCertificateTarget(new ApiCertificates.CertificateService.CertificateServiceClient(channel)),
            ["swap-request-lifecycle"] = new ReplayCandidatesMessageSwapRequestLifecycleWriter(new ApiScheduling.SwapRequestLifecycleService.SwapRequestLifecycleServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "swap-request-lifecycle")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
