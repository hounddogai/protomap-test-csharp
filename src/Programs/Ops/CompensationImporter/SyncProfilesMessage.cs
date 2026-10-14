using Grpc.Net.Client;
using ApiCommon = Acme.Common.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLending = Acme.Lending.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPersonalization = Acme.Personalization.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where SyncProfilesMessage hands each record.</summary>
internal interface ISyncProfilesMessageTarget
{
    Task ApplyAsync(SyncProfilesMessageRecord record, CancellationToken token);
}

/// <summary>What SyncProfilesMessage keeps of each CountProfiles response.</summary>
internal sealed class SyncProfilesMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.itsm.v1.ProblemSyncService.</summary>
internal sealed class SyncProfilesMessageProblemSyncWriter(ApiItsm.ProblemSyncService.ProblemSyncServiceClient client) : ISyncProfilesMessageTarget
{
    public async Task ApplyAsync(SyncProfilesMessageRecord record, CancellationToken token)
    {
        await client.ListProblemConflictsAsync(new ApiItsm.ListProblemConflictsRequest { Filter = new ApiItsm.ListProblemConflictsRequest.Types.Filter { Query = record.Status }, OrderBy = record.Message, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.lending.v1.LoanApplicationCalculationService.</summary>
internal sealed class SyncProfilesMessageLoanApplicationCalculationTarget(ApiLending.LoanApplicationCalculationService.LoanApplicationCalculationServiceClient client) : ISyncProfilesMessageTarget
{
    public async Task ApplyAsync(SyncProfilesMessageRecord record, CancellationToken token)
    {
        await client.CalculateLoanApplicationAsync(new ApiLending.CalculateLoanApplicationRequest { LoanApplicationId = record.Message, LoanApplication = new ApiLending.LoanApplication { LoanApplicationId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.FeedbackValidationService.</summary>
internal sealed class SyncProfilesMessageFeedbackValidationTarget : ISyncProfilesMessageTarget
{
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _client;

    public SyncProfilesMessageFeedbackValidationTarget(ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncProfilesMessageRecord record, CancellationToken token)
    {
        await _client.ScoreFeedbackAsync(new ApiPerformance.ScoreFeedbackRequest { Feedback = new ApiPerformance.Feedback { Kind = record.Status }, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.personalization.v1.ProfileQueryService/CountProfiles and hands what it keeps to the sink of its kind.</summary>
internal static class SyncProfilesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient(channel);
        var found = await source.CountProfilesAsync(new ApiPersonalization.CountProfilesRequest() { OrderBy = "primary-sync-profiles-message" }, cancellationToken: token);
        var record = new SyncProfilesMessageRecord
        {
            Message = found.Result.Message,
            Status = found.Result.Status,
            CorrelationId = found.Result.CorrelationId,
        };
        var sinks = new List<ISyncProfilesMessageTarget>
        {
            new SyncProfilesMessageProblemSyncWriter(new ApiItsm.ProblemSyncService.ProblemSyncServiceClient(channel)),
            new SyncProfilesMessageLoanApplicationCalculationTarget(new ApiLending.LoanApplicationCalculationService.LoanApplicationCalculationServiceClient(channel)),
            new SyncProfilesMessageFeedbackValidationTarget(new ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.ApplyAsync(record, token);
    }
}
