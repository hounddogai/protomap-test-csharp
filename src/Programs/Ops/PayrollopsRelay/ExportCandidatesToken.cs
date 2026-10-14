using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>What ExportCandidatesToken keeps of each SuggestCandidates response.</summary>
internal sealed class ExportCandidatesTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.recruiting.v1.InterviewValidationService.</summary>
internal sealed class ExportCandidatesTokenInterviewValidationSink
{
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _client;

    public ExportCandidatesTokenInterviewValidationSink(ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(ExportCandidatesTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.CheckInterviewEligibilityRequest { Interview = new ApiRecruiting.Interview { Title = record.NextPageToken }, Reason = record.Message }, token, _client.CheckInterviewEligibilityAsync);
    }
}

/// <summary>Reads acme.recruiting.v1.CandidateQueryService/SuggestCandidates and hands what it keeps to acme.recruiting.v1.InterviewValidationService.</summary>
internal static class ExportCandidatesToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient(channel);
        var reply = await source.SuggestCandidatesAsync(new ApiRecruiting.SuggestCandidatesRequest() { OrderBy = "primary-export-candidates-token" }, cancellationToken: token);
        var record = new ExportCandidatesTokenRecord
        {
            NextPageToken = reply.PageInfo.NextPageToken,
            Message = reply.Result.Message,
        };
        await new ExportCandidatesTokenInterviewValidationSink(new ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient(channel)).ApplyAsync(record, token);
    }
}
