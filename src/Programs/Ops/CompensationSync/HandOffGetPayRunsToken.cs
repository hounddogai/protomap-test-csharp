using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiPayroll = Acme.Payroll.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiTelehealth = Acme.Telehealth.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where HandOffGetPayRunsToken hands each record.</summary>
internal interface IHandOffGetPayRunsTokenWriter
{
    Task SendAsync(HandOffGetPayRunsTokenRecord record, CancellationToken token);
}

/// <summary>What HandOffGetPayRunsToken keeps of each BatchGetPayRuns response.</summary>
internal sealed class HandOffGetPayRunsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.telehealth.v1.VideoVisitQueryService.</summary>
internal sealed class HandOffGetPayRunsTokenVideoVisitQueryWriter : IHandOffGetPayRunsTokenWriter
{
    private readonly ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient _client;

    public HandOffGetPayRunsTokenVideoVisitQueryWriter(ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient client) => _client = client;

    public async Task SendAsync(HandOffGetPayRunsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTelehealth.SuggestVideoVisitsRequest { Filter = new ApiTelehealth.SuggestVideoVisitsRequest.Types.Filter { OwnerId = record.NextPageToken }, OrderBy = record.Message }, token, _client.SuggestVideoVisitsAsync);
    }
}

/// <summary>Hands records to acme.performance.v1.ReviewApprovalService.</summary>
internal sealed class HandOffGetPayRunsTokenReviewApprovalTarget : IHandOffGetPayRunsTokenWriter
{
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _client;

    public HandOffGetPayRunsTokenReviewApprovalTarget(ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient client) => _client = client;

    public async Task SendAsync(HandOffGetPayRunsTokenRecord record, CancellationToken token)
    {
        await _client.SubmitReviewAsync(new ApiPerformance.SubmitReviewRequest { Review = new ApiPerformance.Review { Number = record.Message }, ReviewId = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.benefits.v1.LifeEventValidationService.</summary>
internal sealed class HandOffGetPayRunsTokenLifeEventValidationTarget(ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient client) : IHandOffGetPayRunsTokenWriter
{
    public async Task SendAsync(HandOffGetPayRunsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBenefits.ScoreLifeEventRequest { LifeEventId = record.CorrelationId, LifeEvent = new ApiBenefits.LifeEvent { LifeEventId = record.Status } }, token, client.ScoreLifeEventAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffGetPayRunsTokenLogOnly : IHandOffGetPayRunsTokenWriter
{
    public Task SendAsync(HandOffGetPayRunsTokenRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.NextPageToken}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.payroll.v1.PayRunService/BatchGetPayRuns and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffGetPayRunsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPayroll.PayRunService.PayRunServiceClient(channel);
        var response = await source.BatchGetPayRunsAsync(new ApiPayroll.BatchGetPayRunsRequest(), cancellationToken: token);
        var record = new HandOffGetPayRunsTokenRecord
        {
            NextPageToken = response.PageInfo.NextPageToken,
            Status = Text.Normalize(response.Result.Status),
            Message = Text.Normalize(response.Result.Message),
            CorrelationId = response.Result.CorrelationId,
        };
        await DispatchHandOffGetPayRunsTokenAsync("review-approval", record, channel, token);
    }

    private static async Task DispatchHandOffGetPayRunsTokenAsync(string kind, HandOffGetPayRunsTokenRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<HandOffGetPayRunsTokenRecord, CancellationToken, Task> write = new HandOffGetPayRunsTokenLogOnly().SendAsync;
        switch (kind)
        {
            case "video-visit-query":
                write = new HandOffGetPayRunsTokenVideoVisitQueryWriter(new ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient(channel)).SendAsync;
                break;
            case "review-approval":
                write = new HandOffGetPayRunsTokenReviewApprovalTarget(new ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient(channel)).SendAsync;
                break;
            case "life-event-validation":
                write = new HandOffGetPayRunsTokenLifeEventValidationTarget(new ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient(channel)).SendAsync;
                break;
        }
        await write(record, token);
    }
}
