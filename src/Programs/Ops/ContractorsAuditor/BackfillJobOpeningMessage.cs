using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiLabeling = Acme.Labeling.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where BackfillJobOpeningMessage hands each record.</summary>
internal interface IBackfillJobOpeningMessageWriter
{
    Task PushAsync(BackfillJobOpeningMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillJobOpeningMessage keeps of each PreviewJobOpening response.</summary>
internal sealed class BackfillJobOpeningMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.recruiting.v1.CandidateCalculationService.</summary>
internal sealed class BackfillJobOpeningMessageCandidateCalculationRelay(ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient client) : IBackfillJobOpeningMessageWriter
{
    public async Task PushAsync(BackfillJobOpeningMessageRecord record, CancellationToken token)
    {
        await client.QuoteCandidateAsync(new ApiRecruiting.QuoteCandidateRequest { Candidate = new ApiRecruiting.Candidate { CandidateId = record.Status }, Details = new ApiRecruiting.QuoteCandidateRequest.Types.Details { CurrencyCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.TripQueryService.</summary>
internal sealed class BackfillJobOpeningMessageTripQuerySink(ApiTravel.TripQueryService.TripQueryServiceClient client) : IBackfillJobOpeningMessageWriter
{
    public async Task PushAsync(BackfillJobOpeningMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.PeekTripRequest { TripId = record.Message }, token, client.PeekTripAsync);
    }
}

/// <summary>Hands records to acme.labeling.v1.LabelingTaskQueryService.</summary>
internal sealed class BackfillJobOpeningMessageLabelingTaskQuerySink(ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient client) : IBackfillJobOpeningMessageWriter
{
    public async Task PushAsync(BackfillJobOpeningMessageRecord record, CancellationToken token)
    {
        await client.FindLabelingTasksAsync(new ApiLabeling.FindLabelingTasksRequest { OrderBy = record.Status, Filter = new ApiLabeling.FindLabelingTasksRequest.Types.Filter { Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillJobOpeningMessageLogOnly : IBackfillJobOpeningMessageWriter
{
    public Task PushAsync(BackfillJobOpeningMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.recruiting.v1.JobOpeningValidationService/PreviewJobOpening and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillJobOpeningMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient(channel);
        var reply = await source.PreviewJobOpeningAsync(new ApiRecruiting.PreviewJobOpeningRequest() { JobOpeningId = "current-backfill-job-opening-message" }, cancellationToken: token);
        var record = new BackfillJobOpeningMessageRecord
        {
            Message = reply.Result.Message,
            Status = reply.Result.Status,
        };
        await DispatchBackfillJobOpeningMessageAsync("candidate-calculation", record, channel, token);
    }

    private static async Task DispatchBackfillJobOpeningMessageAsync(string kind, BackfillJobOpeningMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<BackfillJobOpeningMessageRecord, CancellationToken, Task> write = new BackfillJobOpeningMessageLogOnly().PushAsync;
        switch (kind)
        {
            case "candidate-calculation":
                write = new BackfillJobOpeningMessageCandidateCalculationRelay(new ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient(channel)).PushAsync;
                break;
            case "trip-query":
                write = new BackfillJobOpeningMessageTripQuerySink(new ApiTravel.TripQueryService.TripQueryServiceClient(channel)).PushAsync;
                break;
            case "labeling-task-query":
                write = new BackfillJobOpeningMessageLabelingTaskQuerySink(new ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient(channel)).PushAsync;
                break;
        }
        await write(record, token);
    }
}
