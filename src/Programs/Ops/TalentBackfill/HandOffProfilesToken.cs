using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>What HandOffProfilesToken keeps of each SuggestProfiles response.</summary>
internal sealed class HandOffProfilesTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.recruiting.v1.CandidateValidationService.</summary>
internal sealed class HandOffProfilesTokenCandidateValidationRelay
{
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _client;

    public HandOffProfilesTokenCandidateValidationRelay(ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient client) => _client = client;

    public async Task PushAsync(HandOffProfilesTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.LintCandidatesRequest { CandidateId = record.Message, Reason = record.NextPageToken, Details = new ApiRecruiting.LintCandidatesRequest.Types.Details { Locale = record.Status } }, token, _client.LintCandidatesAsync);
    }
}

/// <summary>Reads acme.personalization.v1.ProfileQueryService/SuggestProfiles and hands what it keeps to acme.recruiting.v1.CandidateValidationService.</summary>
internal static class HandOffProfilesToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient(channel);
        var reply = await source.SuggestProfilesAsync(new ApiPersonalization.SuggestProfilesRequest() { OrderBy = "nightly-hand-off-profiles-token" }, cancellationToken: token);
        var record = new HandOffProfilesTokenRecord
        {
            NextPageToken = reply.PageInfo.NextPageToken,
            CorrelationId = reply.Result.CorrelationId,
            Message = reply.Result.Message,
            Status = reply.Result.Status,
        };
        await new HandOffProfilesTokenCandidateValidationRelay(new ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient(channel)).PushAsync(record, token);
    }
}
