using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiChat = Acme.Chat.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where CopyPatientsMessage hands each record.</summary>
internal interface ICopyPatientsMessageTarget
{
    Task SendAsync(CopyPatientsMessageRecord record, CancellationToken token);
}

/// <summary>What CopyPatientsMessage keeps of each CountPatients response.</summary>
internal sealed class CopyPatientsMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.chat.v1.ChatRoomCalculationService.</summary>
internal sealed class CopyPatientsMessageChatRoomCalculationTarget(ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient client) : ICopyPatientsMessageTarget
{
    public async Task SendAsync(CopyPatientsMessageRecord record, CancellationToken token)
    {
        await client.ExplainChatRoomCalculationAsync(new ApiChat.ExplainChatRoomCalculationRequest { Reason = record.Status, Details = new ApiChat.ExplainChatRoomCalculationRequest.Types.Details { Locale = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentValidationService.</summary>
internal sealed class CopyPatientsMessageDocumentValidationWriter(ApiDocuments.DocumentValidationService.DocumentValidationServiceClient client) : ICopyPatientsMessageTarget
{
    public async Task SendAsync(CopyPatientsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.CheckDocumentEligibilityRequest { DocumentId = record.Status }, token, client.CheckDocumentEligibilityAsync);
    }
}

/// <summary>Hands records to acme.featureflags.v1.FeatureFlagQueryService.</summary>
internal sealed class CopyPatientsMessageFeatureFlagQueryRelay(ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient client) : ICopyPatientsMessageTarget
{
    public async Task SendAsync(CopyPatientsMessageRecord record, CancellationToken token)
    {
        await client.PeekFeatureFlagAsync(new ApiFeatureflags.PeekFeatureFlagRequest { FeatureFlagId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.patients.v1.PatientQueryService/CountPatients and hands what it keeps to the sink of its kind.</summary>
internal static class CopyPatientsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPatients.PatientQueryService.PatientQueryServiceClient(channel);
        var found = await source.CountPatientsAsync(new ApiPatients.CountPatientsRequest() { OrderBy = "nightly-copy-patients-message" }, cancellationToken: token);
        var record = new CopyPatientsMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            Status = found.Result.Status,
        };
        var sinks = new Dictionary<string, ICopyPatientsMessageTarget>
        {
            ["chat-room-calculation"] = new CopyPatientsMessageChatRoomCalculationTarget(new ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient(channel)),
            ["document-validation"] = new CopyPatientsMessageDocumentValidationWriter(new ApiDocuments.DocumentValidationService.DocumentValidationServiceClient(channel)),
            ["feature-flag-query"] = new CopyPatientsMessageFeatureFlagQueryRelay(new ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "document-validation")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
