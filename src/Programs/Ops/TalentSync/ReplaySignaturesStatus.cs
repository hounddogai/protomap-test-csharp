using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where ReplaySignaturesStatus hands each record.</summary>
internal interface IReplaySignaturesStatusSink
{
    Task DeliverAsync(ReplaySignaturesStatusRecord record, CancellationToken token);
}

/// <summary>What ReplaySignaturesStatus keeps of each PullSignatures response.</summary>
internal sealed class ReplaySignaturesStatusRecord
{
    public string Status { get; init; } = "";
    public string SignatureId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.itsm.v1.ProblemApprovalService.</summary>
internal sealed class ReplaySignaturesStatusProblemApprovalSink(ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient client) : IReplaySignaturesStatusSink
{
    public async Task DeliverAsync(ReplaySignaturesStatusRecord record, CancellationToken token)
    {
        await client.ListPendingProblemsAsync(new ApiItsm.ListPendingProblemsRequest { Filter = new ApiItsm.ListPendingProblemsRequest.Types.Filter { Query = record.CorrelationId, OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.learning.v1.CourseApprovalService.</summary>
internal sealed class ReplaySignaturesStatusCourseApprovalRelay(ApiLearning.CourseApprovalService.CourseApprovalServiceClient client) : IReplaySignaturesStatusSink
{
    public async Task DeliverAsync(ReplaySignaturesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.EscalateCourseRequest { Course = new ApiLearning.Course { Name = record.Status, Code = record.SignatureId }, Details = new ApiLearning.EscalateCourseRequest.Types.Details { Comment = record.CorrelationId } }, token, client.EscalateCourseAsync);
    }
}

/// <summary>Hands records to acme.sellers.v1.SellerQueryService.</summary>
internal sealed class ReplaySignaturesStatusSellerQuerySink : IReplaySignaturesStatusSink
{
    private readonly ApiSellers.SellerQueryService.SellerQueryServiceClient _client;

    public ReplaySignaturesStatusSellerQuerySink(ApiSellers.SellerQueryService.SellerQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(ReplaySignaturesStatusRecord record, CancellationToken token)
    {
        await _client.SuggestSellersAsync(new ApiSellers.SuggestSellersRequest { Filter = new ApiSellers.SuggestSellersRequest.Types.Filter { OwnerId = record.CorrelationId, Query = record.SignatureId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplaySignaturesStatusLogOnly : IReplaySignaturesStatusSink
{
    public Task DeliverAsync(ReplaySignaturesStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.SignatureSyncService/PullSignatures and hands what it keeps to the sink of its kind.</summary>
internal static class ReplaySignaturesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.SignatureSyncService.SignatureSyncServiceClient(channel);
        var reply = await source.PullSignaturesAsync(new ApiDocuments.PullSignaturesRequest() { SignatureId = "nightly-replay-signatures-status" }, cancellationToken: token);
        var record = new ReplaySignaturesStatusRecord
        {
            Status = reply.Result.Status,
            SignatureId = reply.Signature.SignatureId,
            CorrelationId = reply.Signature.CorrelationId,
        };
        await RouteReplaySignaturesStatusAsync("problem-approval", record, channel, token);
    }

    private static async Task RouteReplaySignaturesStatusAsync(string kind, ReplaySignaturesStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IReplaySignaturesStatusSink sink = new ReplaySignaturesStatusLogOnly();
        switch (kind)
        {
            case "problem-approval":
                sink = new ReplaySignaturesStatusProblemApprovalSink(new ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient(channel));
                break;
            case "course-approval":
                sink = new ReplaySignaturesStatusCourseApprovalRelay(new ApiLearning.CourseApprovalService.CourseApprovalServiceClient(channel));
                break;
            case "seller-query":
                sink = new ReplaySignaturesStatusSellerQuerySink(new ApiSellers.SellerQueryService.SellerQueryServiceClient(channel));
                break;
        }
        await sink.DeliverAsync(record, token);
    }
}
