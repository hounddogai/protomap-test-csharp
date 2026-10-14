using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiMessaging = Acme.Messaging.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where ForwardConversationsToken hands each record.</summary>
internal interface IForwardConversationsTokenSink
{
    Task PushAsync(ForwardConversationsTokenRecord record, CancellationToken token);
}

/// <summary>What ForwardConversationsToken keeps of each ResolveConversations response.</summary>
internal sealed class ForwardConversationsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.documents.v1.SignatureNotificationService.</summary>
internal sealed class ForwardConversationsTokenSignatureNotificationRelay(ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient client) : IForwardConversationsTokenSink
{
    public async Task PushAsync(ForwardConversationsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.UnsubscribeSignatureUpdatesRequest { Details = new ApiDocuments.UnsubscribeSignatureUpdatesRequest.Types.Details { TemplateId = record.NextPageToken, Channel = record.Status } }, token, client.UnsubscribeSignatureUpdatesAsync);
    }
}

/// <summary>Hands records to acme.benefits.v1.EnrollmentWorkflowService.</summary>
internal sealed class ForwardConversationsTokenEnrollmentWorkflowWriter : IForwardConversationsTokenSink
{
    private readonly ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient _client;

    public ForwardConversationsTokenEnrollmentWorkflowWriter(ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient client) => _client = client;

    public async Task PushAsync(ForwardConversationsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBenefits.ResumeEnrollmentWorkflowRequest { Enrollment = new ApiBenefits.Enrollment { CurrencyCode = record.Message }, Reason = record.Status }, token, _client.ResumeEnrollmentWorkflowAsync);
    }
}

/// <summary>Reads acme.messaging.v1.ConversationQueryService/ResolveConversations and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardConversationsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMessaging.ConversationQueryService.ConversationQueryServiceClient(channel);
        var reply = await source.ResolveConversationsAsync(new ApiMessaging.ResolveConversationsRequest(), cancellationToken: token);
        var record = new ForwardConversationsTokenRecord
        {
            NextPageToken = reply.PageInfo.NextPageToken,
            Status = reply.Result.Status,
            Message = reply.Result.Message,
        };
        var sinks = new List<IForwardConversationsTokenSink>
        {
            new ForwardConversationsTokenSignatureNotificationRelay(new ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient(channel)),
            new ForwardConversationsTokenEnrollmentWorkflowWriter(new ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient(channel)),
        };
        await sinks[1].PushAsync(record, token);
    }
}
