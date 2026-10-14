using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLearning = Acme.Learning.V1;
using ApiSecrets = Acme.Secrets.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where HandOffDocumentFolderRulesId hands each record.</summary>
internal interface IHandOffDocumentFolderRulesIdDestination
{
    Task ForwardAsync(HandOffDocumentFolderRulesIdRecord record, CancellationToken token);
}

/// <summary>What HandOffDocumentFolderRulesId keeps of each GetDocumentFolderRules response.</summary>
internal sealed class HandOffDocumentFolderRulesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string ParentId { get; init; } = "";
    public string DocumentFolderId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.learning.v1.CourseEnrollmentLifecycleService.</summary>
internal sealed class HandOffDocumentFolderRulesIdCourseEnrollmentLifecycleSink(ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient client) : IHandOffDocumentFolderRulesIdDestination
{
    public async Task ForwardAsync(HandOffDocumentFolderRulesIdRecord record, CancellationToken token)
    {
        await client.RenewCourseEnrollmentAsync(new ApiLearning.RenewCourseEnrollmentRequest { Details = new ApiLearning.RenewCourseEnrollmentRequest.Types.Details { ReasonCode = record.DocumentFolderId }, Reason = record.ParentId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.secrets.v1.SecretService.</summary>
internal sealed class HandOffDocumentFolderRulesIdSecretSink : IHandOffDocumentFolderRulesIdDestination
{
    private readonly ApiSecrets.SecretService.SecretServiceClient _client;

    public HandOffDocumentFolderRulesIdSecretSink(ApiSecrets.SecretService.SecretServiceClient client) => _client = client;

    public async Task ForwardAsync(HandOffDocumentFolderRulesIdRecord record, CancellationToken token)
    {
        await _client.UpdateSecretAsync(new ApiSecrets.UpdateSecretRequest { Secret = new ApiSecrets.Secret { SecretId = record.CorrelationId, CurrencyCode = record.DocumentFolderId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.corporateevents.v1.RegistrationLifecycleService.</summary>
internal sealed class HandOffDocumentFolderRulesIdRegistrationLifecycleTarget(ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient client) : IHandOffDocumentFolderRulesIdDestination
{
    public async Task ForwardAsync(HandOffDocumentFolderRulesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.ExpireRegistrationRequest { Details = new ApiCorporateevents.ExpireRegistrationRequest.Types.Details { ReasonCode = record.CorrelationId }, Reason = record.ParentId }, token, client.ExpireRegistrationAsync);
    }
}

/// <summary>Passes records to acme.corporateevents.v1.CorporateEventService.</summary>
internal sealed class HandOffDocumentFolderRulesIdCorporateEventTarget(ApiCorporateevents.CorporateEventService.CorporateEventServiceClient client) : IHandOffDocumentFolderRulesIdDestination
{
    public async Task ForwardAsync(HandOffDocumentFolderRulesIdRecord record, CancellationToken token)
    {
        await client.CreateCorporateEventAsync(new ApiCorporateevents.CreateCorporateEventRequest { CorporateEvent = new ApiCorporateevents.CorporateEvent { Title = record.Status, CorporateEventId = record.DocumentFolderId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentFolderValidationService/GetDocumentFolderRules and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffDocumentFolderRulesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient(channel);
        var got = await source.GetDocumentFolderRulesAsync(new ApiDocuments.GetDocumentFolderRulesRequest() { DocumentFolderId = "primary-hand-off-document-folder-rules-id" }, cancellationToken: token);
        var record = new HandOffDocumentFolderRulesIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            ParentId = got.DocumentFolder.ParentId,
            DocumentFolderId = got.DocumentFolder.DocumentFolderId,
            Status = got.Result.Status,
        };
        var sinks = new List<IHandOffDocumentFolderRulesIdDestination>
        {
            new HandOffDocumentFolderRulesIdCourseEnrollmentLifecycleSink(new ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient(channel)),
            new HandOffDocumentFolderRulesIdSecretSink(new ApiSecrets.SecretService.SecretServiceClient(channel)),
            new HandOffDocumentFolderRulesIdRegistrationLifecycleTarget(new ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient(channel)),
            new HandOffDocumentFolderRulesIdCorporateEventTarget(new ApiCorporateevents.CorporateEventService.CorporateEventServiceClient(channel)),
        };
        await sinks[1].ForwardAsync(record, token);
    }
}
