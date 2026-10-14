using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiReturns = Acme.Returns.V1;
using ApiShipping = Acme.Shipping.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where ForwardShipmentsStatus hands each record.</summary>
internal interface IForwardShipmentsStatusDestination
{
    Task SendAsync(ForwardShipmentsStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardShipmentsStatus keeps of each ListShipments response.</summary>
internal sealed class ForwardShipmentsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.returns.v1.ReturnRequestQueryService.</summary>
internal sealed class ForwardShipmentsStatusReturnRequestQueryRelay(ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient client) : IForwardShipmentsStatusDestination
{
    public async Task SendAsync(ForwardShipmentsStatusRecord record, CancellationToken token)
    {
        await client.SummarizeReturnRequestsAsync(new ApiReturns.SummarizeReturnRequestsRequest { Reason = record.CorrelationId, ReturnRequestId = record.Status, Details = new ApiReturns.SummarizeReturnRequestsRequest.Types.Details { Format = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.learning.v1.CourseWorkflowService.</summary>
internal sealed class ForwardShipmentsStatusCourseWorkflowTarget(ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient client) : IForwardShipmentsStatusDestination
{
    public async Task SendAsync(ForwardShipmentsStatusRecord record, CancellationToken token)
    {
        await client.InspectCourseWorkflowAsync(new ApiLearning.InspectCourseWorkflowRequest { CourseId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.InterviewApprovalService.</summary>
internal sealed class ForwardShipmentsStatusInterviewApprovalWriter : IForwardShipmentsStatusDestination
{
    private readonly ApiRecruiting.InterviewApprovalService.InterviewApprovalServiceClient _client;

    public ForwardShipmentsStatusInterviewApprovalWriter(ApiRecruiting.InterviewApprovalService.InterviewApprovalServiceClient client) => _client = client;

    public async Task SendAsync(ForwardShipmentsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.ListPendingInterviewsRequest { Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId }, Filter = new ApiRecruiting.ListPendingInterviewsRequest.Types.Filter { Region = record.Message } }, token, _client.ListPendingInterviewsAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardShipmentsStatusLogOnly : IForwardShipmentsStatusDestination
{
    public Task SendAsync(ForwardShipmentsStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.shipping.v1.ShipmentService/ListShipments and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardShipmentsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiShipping.ShipmentService.ShipmentServiceClient(channel);
        var reply = await source.ListShipmentsAsync(new ApiShipping.ListShipmentsRequest() { OrderBy = "main-forward-shipments-status" }, cancellationToken: token);
        var record = new ForwardShipmentsStatusRecord
        {
            Status = reply.Result.Status,
            Message = reply.Result.Message,
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
        };
        await DispatchForwardShipmentsStatusAsync("course-workflow", record, channel, token);
    }

    private static async Task DispatchForwardShipmentsStatusAsync(string kind, ForwardShipmentsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<ForwardShipmentsStatusRecord, CancellationToken, Task> write = new ForwardShipmentsStatusLogOnly().SendAsync;
        switch (kind)
        {
            case "return-request-query":
                write = new ForwardShipmentsStatusReturnRequestQueryRelay(new ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient(channel)).SendAsync;
                break;
            case "course-workflow":
                write = new ForwardShipmentsStatusCourseWorkflowTarget(new ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient(channel)).SendAsync;
                break;
            case "interview-approval":
                write = new ForwardShipmentsStatusInterviewApprovalWriter(new ApiRecruiting.InterviewApprovalService.InterviewApprovalServiceClient(channel)).SendAsync;
                break;
        }
        await write(record, token);
    }
}
