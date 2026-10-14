using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiFraud = Acme.Fraud.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where BackfillChatRoomsId hands each record.</summary>
internal interface IBackfillChatRoomsIdDestination
{
    Task ForwardAsync(BackfillChatRoomsIdRecord record, CancellationToken token);
}

/// <summary>What BackfillChatRoomsId keeps of each ResolveChatRooms response.</summary>
internal sealed class BackfillChatRoomsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.benefits.v1.BenefitPlanWorkflowService.</summary>
internal sealed class BackfillChatRoomsIdBenefitPlanWorkflowTarget(ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient client) : IBackfillChatRoomsIdDestination
{
    public async Task ForwardAsync(BackfillChatRoomsIdRecord record, CancellationToken token)
    {
        await client.PauseBenefitPlanWorkflowAsync(new ApiBenefits.PauseBenefitPlanWorkflowRequest { BenefitPlan = new ApiBenefits.BenefitPlan { Name = record.Message }, Details = new ApiBenefits.PauseBenefitPlanWorkflowRequest.Types.Details { InitiatedBy = record.Status }, BenefitPlanId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.deliveries.v1.DeliveryQueryService.</summary>
internal sealed class BackfillChatRoomsIdDeliveryQueryTarget(ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient client) : IBackfillChatRoomsIdDestination
{
    public async Task ForwardAsync(BackfillChatRoomsIdRecord record, CancellationToken token)
    {
        await client.CountDeliveriesAsync(new ApiDeliveries.CountDeliveriesRequest { Page = new ApiCommon.PageRequest { PageToken = record.Message }, Filter = new ApiDeliveries.CountDeliveriesRequest.Types.Filter { Query = record.CorrelationId, Region = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.fraud.v1.RiskAssessmentCalculationService.</summary>
internal sealed class BackfillChatRoomsIdRiskAssessmentCalculationWriter(ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient client) : IBackfillChatRoomsIdDestination
{
    public async Task ForwardAsync(BackfillChatRoomsIdRecord record, CancellationToken token)
    {
        await client.SimulateRiskAssessmentAsync(new ApiFraud.SimulateRiskAssessmentRequest { RiskAssessment = new ApiFraud.RiskAssessment { Subject = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillChatRoomsIdLogOnly : IBackfillChatRoomsIdDestination
{
    public Task ForwardAsync(BackfillChatRoomsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.chat.v1.ChatRoomQueryService/ResolveChatRooms and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillChatRoomsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient(channel);
        var found = await source.ResolveChatRoomsAsync(new ApiChat.ResolveChatRoomsRequest(), cancellationToken: token);
        var record = new BackfillChatRoomsIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Message = found.Result.Message,
            NextPageToken = Text.Normalize(found.PageInfo.NextPageToken),
            Status = found.Result.Status,
        };
        await DeliverBackfillChatRoomsIdAsync("risk-assessment-calculation", record, channel, token);
    }

    private static async Task DeliverBackfillChatRoomsIdAsync(string kind, BackfillChatRoomsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IBackfillChatRoomsIdDestination sink = new BackfillChatRoomsIdLogOnly();
        switch (kind)
        {
            case "benefit-plan-workflow":
                sink = new BackfillChatRoomsIdBenefitPlanWorkflowTarget(new ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient(channel));
                break;
            case "delivery-query":
                sink = new BackfillChatRoomsIdDeliveryQueryTarget(new ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient(channel));
                break;
            case "risk-assessment-calculation":
                sink = new BackfillChatRoomsIdRiskAssessmentCalculationWriter(new ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
