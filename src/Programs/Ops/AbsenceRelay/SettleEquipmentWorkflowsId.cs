using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where SettleEquipmentWorkflowsId hands each record.</summary>
internal interface ISettleEquipmentWorkflowsIdSink
{
    Task ApplyAsync(SettleEquipmentWorkflowsIdRecord record, CancellationToken token);
}

/// <summary>What SettleEquipmentWorkflowsId keeps of each ListEquipmentWorkflows response.</summary>
internal sealed class SettleEquipmentWorkflowsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.giftcards.v1.GiftCardQueryService.</summary>
internal sealed class SettleEquipmentWorkflowsIdGiftCardQueryTarget(ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient client) : ISettleEquipmentWorkflowsIdSink
{
    public async Task ApplyAsync(SettleEquipmentWorkflowsIdRecord record, CancellationToken token)
    {
        await client.SuggestGiftCardsAsync(new ApiGiftcards.SuggestGiftCardsRequest { Filter = new ApiGiftcards.SuggestGiftCardsRequest.Types.Filter { Region = record.CorrelationId, OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.ItineraryApprovalService.</summary>
internal sealed class SettleEquipmentWorkflowsIdItineraryApprovalRelay(ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient client) : ISettleEquipmentWorkflowsIdSink
{
    public async Task ApplyAsync(SettleEquipmentWorkflowsIdRecord record, CancellationToken token)
    {
        await client.ListPendingItinerariesAsync(new ApiTravel.ListPendingItinerariesRequest { Filter = new ApiTravel.ListPendingItinerariesRequest.Types.Filter { OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.timekeeping.v1.OvertimeValidationService.</summary>
internal sealed class SettleEquipmentWorkflowsIdOvertimeValidationTarget(ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient client) : ISettleEquipmentWorkflowsIdSink
{
    public async Task ApplyAsync(SettleEquipmentWorkflowsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.ScoreOvertimeRequest { Details = new ApiTimekeeping.ScoreOvertimeRequest.Types.Details { CurrencyCode = record.Message }, Reason = record.Status, OvertimeId = record.CorrelationId }, token, client.ScoreOvertimeAsync);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentNotificationService.</summary>
internal sealed class SettleEquipmentWorkflowsIdDocumentNotificationSink : ISettleEquipmentWorkflowsIdSink
{
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _client;

    public SettleEquipmentWorkflowsIdDocumentNotificationSink(ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleEquipmentWorkflowsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.NotifyDocumentWatchersRequest { Details = new ApiDocuments.NotifyDocumentWatchersRequest.Types.Details { TemplateId = record.Status }, DocumentId = record.CorrelationId }, token, _client.NotifyDocumentWatchersAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SettleEquipmentWorkflowsIdLogOnly : ISettleEquipmentWorkflowsIdSink
{
    public Task ApplyAsync(SettleEquipmentWorkflowsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.onboarding.v1.EquipmentWorkflowService/ListEquipmentWorkflows and hands what it keeps to the sink of its kind.</summary>
internal static class SettleEquipmentWorkflowsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient(channel);
        var response = await source.ListEquipmentWorkflowsAsync(new ApiOnboarding.ListEquipmentWorkflowsRequest() { OrderBy = "default-settle-equipment-workflows-id" }, cancellationToken: token);
        var record = new SettleEquipmentWorkflowsIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Status = response.Result.Status,
            Message = response.Result.Message,
        };
        await DeliverSettleEquipmentWorkflowsIdAsync("overtime-validation", record, channel, token);
    }

    private static async Task DeliverSettleEquipmentWorkflowsIdAsync(string kind, SettleEquipmentWorkflowsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISettleEquipmentWorkflowsIdSink sink = kind switch
        {
            "gift-card-query" => new SettleEquipmentWorkflowsIdGiftCardQueryTarget(new ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient(channel)),
            "itinerary-approval" => new SettleEquipmentWorkflowsIdItineraryApprovalRelay(new ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient(channel)),
            "overtime-validation" => new SettleEquipmentWorkflowsIdOvertimeValidationTarget(new ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient(channel)),
            "document-notification" => new SettleEquipmentWorkflowsIdDocumentNotificationSink(new ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient(channel)),
            _ => new SettleEquipmentWorkflowsIdLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
