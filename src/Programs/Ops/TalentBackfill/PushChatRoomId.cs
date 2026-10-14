using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiChat = Acme.Chat.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where PushChatRoomId hands each record.</summary>
internal interface IPushChatRoomIdDestination
{
    Task PushAsync(PushChatRoomIdRecord record, CancellationToken token);
}

/// <summary>What PushChatRoomId keeps of each CloneChatRoom response.</summary>
internal sealed class PushChatRoomIdRecord
{
    public string ChatRoomId { get; init; } = "";
    public string Code { get; init; } = "";
}

/// <summary>Passes records to acme.helpdesk.v1.KnowledgeArticleValidationService.</summary>
internal sealed class PushChatRoomIdKnowledgeArticleValidationSink(ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient client) : IPushChatRoomIdDestination
{
    public async Task PushAsync(PushChatRoomIdRecord record, CancellationToken token)
    {
        await client.ValidateKnowledgeArticleAsync(new ApiHelpdesk.ValidateKnowledgeArticleRequest { KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle { Notes = record.Code }, KnowledgeArticleId = record.ChatRoomId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.performance.v1.FeedbackCalculationService.</summary>
internal sealed class PushChatRoomIdFeedbackCalculationWriter : IPushChatRoomIdDestination
{
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _client;

    public PushChatRoomIdFeedbackCalculationWriter(ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient client) => _client = client;

    public async Task PushAsync(PushChatRoomIdRecord record, CancellationToken token)
    {
        await _client.ExplainFeedbackCalculationAsync(new ApiPerformance.ExplainFeedbackCalculationRequest { Feedback = new ApiPerformance.Feedback { Kind = record.Code, CorrelationId = record.ChatRoomId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.prescriptions.v1.PrescriptionService.</summary>
internal sealed class PushChatRoomIdPrescriptionTarget : IPushChatRoomIdDestination
{
    private readonly ApiPrescriptions.PrescriptionService.PrescriptionServiceClient _client;

    public PushChatRoomIdPrescriptionTarget(ApiPrescriptions.PrescriptionService.PrescriptionServiceClient client) => _client = client;

    public async Task PushAsync(PushChatRoomIdRecord record, CancellationToken token)
    {
        await _client.DeletePrescriptionAsync(new ApiPrescriptions.DeletePrescriptionRequest { Etag = record.Code }, cancellationToken: token);
    }
}

/// <summary>Reads acme.chat.v1.ChatRoomService/CloneChatRoom and hands what it keeps to the sink of its kind.</summary>
internal static class PushChatRoomId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiChat.ChatRoomService.ChatRoomServiceClient(channel);
        var response = await source.CloneChatRoomAsync(new ApiChat.CloneChatRoomRequest() { RequestId = "current-push-chat-room-id" }, cancellationToken: token);
        var record = new PushChatRoomIdRecord
        {
            ChatRoomId = response.ChatRoom.ChatRoomId,
            Code = Text.Normalize(response.ChatRoom.Code),
        };
        var sinks = new Dictionary<string, IPushChatRoomIdDestination>
        {
            ["knowledge-article-validation"] = new PushChatRoomIdKnowledgeArticleValidationSink(new ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient(channel)),
            ["feedback-calculation"] = new PushChatRoomIdFeedbackCalculationWriter(new ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient(channel)),
            ["prescription"] = new PushChatRoomIdPrescriptionTarget(new ApiPrescriptions.PrescriptionService.PrescriptionServiceClient(channel)),
        };
        await RoutePushChatRoomIdAsync("prescription", record, sinks, token);
    }

    private static async Task RoutePushChatRoomIdAsync(string kind, PushChatRoomIdRecord record, IReadOnlyDictionary<string, IPushChatRoomIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.PushAsync(record, token);
    }
}
