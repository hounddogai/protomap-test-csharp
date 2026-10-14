using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where RouteEquipmentWorkflowStatus hands each record.</summary>
internal interface IRouteEquipmentWorkflowStatusWriter
{
    Task WriteAsync(RouteEquipmentWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What RouteEquipmentWorkflowStatus keeps of each PauseEquipmentWorkflow response.</summary>
internal sealed class RouteEquipmentWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.messaging.v1.ConversationService.</summary>
internal sealed class RouteEquipmentWorkflowStatusConversationRelay(ApiMessaging.ConversationService.ConversationServiceClient client) : IRouteEquipmentWorkflowStatusWriter
{
    public async Task WriteAsync(RouteEquipmentWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiMessaging.ListConversationsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Message }, OrderBy = record.Code }, token, client.ListConversationsAsync);
    }
}

/// <summary>Passes records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class RouteEquipmentWorkflowStatusCheckoutSessionWriter : IRouteEquipmentWorkflowStatusWriter
{
    private readonly ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient _client;

    public RouteEquipmentWorkflowStatusCheckoutSessionWriter(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) => _client = client;

    public async Task WriteAsync(RouteEquipmentWorkflowStatusRecord record, CancellationToken token)
    {
        await _client.GetCheckoutSessionAsync(new ApiCheckout.GetCheckoutSessionRequest { CheckoutSessionId = record.Code }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.RosterWorkflowService.</summary>
internal sealed class RouteEquipmentWorkflowStatusRosterWorkflowSink(ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient client) : IRouteEquipmentWorkflowStatusWriter
{
    public async Task WriteAsync(RouteEquipmentWorkflowStatusRecord record, CancellationToken token)
    {
        await client.CancelRosterWorkflowAsync(new ApiScheduling.CancelRosterWorkflowRequest { RosterId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.helpdesk.v1.KnowledgeArticleQueryService.</summary>
internal sealed class RouteEquipmentWorkflowStatusKnowledgeArticleQueryRelay : IRouteEquipmentWorkflowStatusWriter
{
    private readonly ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient _client;

    public RouteEquipmentWorkflowStatusKnowledgeArticleQueryRelay(ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient client) => _client = client;

    public async Task WriteAsync(RouteEquipmentWorkflowStatusRecord record, CancellationToken token)
    {
        await _client.PeekKnowledgeArticleAsync(new ApiHelpdesk.PeekKnowledgeArticleRequest { KnowledgeArticleId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.onboarding.v1.EquipmentWorkflowService/PauseEquipmentWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class RouteEquipmentWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient(channel);
        var got = await source.PauseEquipmentWorkflowAsync(new ApiOnboarding.PauseEquipmentWorkflowRequest() { EquipmentId = "nightly-route-equipment-workflow-status" }, cancellationToken: token);
        var record = new RouteEquipmentWorkflowStatusRecord
        {
            Status = got.Result.Status,
            Code = Text.Normalize(got.Equipment.Code),
            Message = got.Result.Message,
        };
        var sinks = new Dictionary<string, IRouteEquipmentWorkflowStatusWriter>
        {
            ["conversation"] = new RouteEquipmentWorkflowStatusConversationRelay(new ApiMessaging.ConversationService.ConversationServiceClient(channel)),
            ["checkout-session"] = new RouteEquipmentWorkflowStatusCheckoutSessionWriter(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel)),
            ["roster-workflow"] = new RouteEquipmentWorkflowStatusRosterWorkflowSink(new ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient(channel)),
            ["knowledge-article-query"] = new RouteEquipmentWorkflowStatusKnowledgeArticleQueryRelay(new ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient(channel)),
        };
        await RouteRouteEquipmentWorkflowStatusAsync("roster-workflow", record, sinks, token);
    }

    private static async Task RouteRouteEquipmentWorkflowStatusAsync(string kind, RouteEquipmentWorkflowStatusRecord record, IReadOnlyDictionary<string, IRouteEquipmentWorkflowStatusWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
