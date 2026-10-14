using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where CarryOrgUnitWorkflowMessage hands each record.</summary>
internal interface ICarryOrgUnitWorkflowMessageDestination
{
    Task SendAsync(CarryOrgUnitWorkflowMessageRecord record, CancellationToken token);
}

/// <summary>What CarryOrgUnitWorkflowMessage keeps of each RetryOrgUnitWorkflow response.</summary>
internal sealed class CarryOrgUnitWorkflowMessageRecord
{
    public string Message { get; init; } = "";
    public string ParentId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.sellers.v1.SellerService.</summary>
internal sealed class CarryOrgUnitWorkflowMessageSellerSink(ApiSellers.SellerService.SellerServiceClient client) : ICarryOrgUnitWorkflowMessageDestination
{
    public async Task SendAsync(CarryOrgUnitWorkflowMessageRecord record, CancellationToken token)
    {
        await client.CloneSellerAsync(new ApiSellers.CloneSellerRequest { Seller = new ApiSellers.Seller { SellerId = record.ParentId }, RequestId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.scheduling.v1.SwapRequestApprovalService.</summary>
internal sealed class CarryOrgUnitWorkflowMessageSwapRequestApprovalTarget(ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient client) : ICarryOrgUnitWorkflowMessageDestination
{
    public async Task SendAsync(CarryOrgUnitWorkflowMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.ApproveSwapRequestRequest { Details = new ApiScheduling.ApproveSwapRequestRequest.Types.Details { Comment = record.ParentId }, SwapRequestId = record.Message, SwapRequest = new ApiScheduling.SwapRequest { SwapRequestId = record.Status } }, token, client.ApproveSwapRequestAsync);
    }
}

/// <summary>Hands records to acme.messaging.v1.ConversationService.</summary>
internal sealed class CarryOrgUnitWorkflowMessageConversationRelay : ICarryOrgUnitWorkflowMessageDestination
{
    private readonly ApiMessaging.ConversationService.ConversationServiceClient _client;

    public CarryOrgUnitWorkflowMessageConversationRelay(ApiMessaging.ConversationService.ConversationServiceClient client) => _client = client;

    public async Task SendAsync(CarryOrgUnitWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.UpdateConversationAsync(new ApiMessaging.UpdateConversationRequest { Conversation = new ApiMessaging.Conversation { Subject = record.ParentId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitWorkflowService/RetryOrgUnitWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class CarryOrgUnitWorkflowMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel);
        var got = await source.RetryOrgUnitWorkflowAsync(new ApiEmployees.RetryOrgUnitWorkflowRequest() { Reason = "live-carry-org-unit-workflow-message" }, cancellationToken: token);
        var record = new CarryOrgUnitWorkflowMessageRecord
        {
            Message = got.Outcome.Message,
            ParentId = Text.Normalize(got.OrgUnit.ParentId),
            Status = got.Outcome.Status,
        };
        var sinks = new Dictionary<string, ICarryOrgUnitWorkflowMessageDestination>
        {
            ["seller"] = new CarryOrgUnitWorkflowMessageSellerSink(new ApiSellers.SellerService.SellerServiceClient(channel)),
            ["swap-request-approval"] = new CarryOrgUnitWorkflowMessageSwapRequestApprovalTarget(new ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient(channel)),
            ["conversation"] = new CarryOrgUnitWorkflowMessageConversationRelay(new ApiMessaging.ConversationService.ConversationServiceClient(channel)),
        };
        await RouteCarryOrgUnitWorkflowMessageAsync("seller", record, sinks, token);
    }

    private static async Task RouteCarryOrgUnitWorkflowMessageAsync(string kind, CarryOrgUnitWorkflowMessageRecord record, IReadOnlyDictionary<string, ICarryOrgUnitWorkflowMessageDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
