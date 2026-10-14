using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAudit = Acme.Audit.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where MirrorCalibrationId hands each record.</summary>
internal interface IMirrorCalibrationIdSink
{
    Task ForwardAsync(MirrorCalibrationIdRecord record, CancellationToken token);
}

/// <summary>What MirrorCalibrationId keeps of each NormalizeCalibration response.</summary>
internal sealed class MirrorCalibrationIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentFolderValidationService.</summary>
internal sealed class MirrorCalibrationIdDocumentFolderValidationSink(ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient client) : IMirrorCalibrationIdSink
{
    public async Task ForwardAsync(MirrorCalibrationIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.ValidateDocumentFolderRequest { DocumentFolder = new ApiDocuments.DocumentFolder { DocumentFolderId = record.CorrelationId } }, token, client.ValidateDocumentFolderAsync);
    }
}

/// <summary>Passes records to acme.performance.v1.ReviewService.</summary>
internal sealed class MirrorCalibrationIdReviewSink(ApiPerformance.ReviewService.ReviewServiceClient client) : IMirrorCalibrationIdSink
{
    public async Task ForwardAsync(MirrorCalibrationIdRecord record, CancellationToken token)
    {
        await client.CreateReviewAsync(new ApiPerformance.CreateReviewRequest { Review = new ApiPerformance.Review { ReviewId = record.Subject, Number = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.audit.v1.AuditEventQueryService.</summary>
internal sealed class MirrorCalibrationIdAuditEventQuerySink(ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient client) : IMirrorCalibrationIdSink
{
    public async Task ForwardAsync(MirrorCalibrationIdRecord record, CancellationToken token)
    {
        await client.LookupAuditEventAsync(new ApiAudit.LookupAuditEventRequest { AuditEventId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.surveys.v1.SurveyService.</summary>
internal sealed class MirrorCalibrationIdSurveyTarget : IMirrorCalibrationIdSink
{
    private readonly ApiSurveys.SurveyService.SurveyServiceClient _client;

    public MirrorCalibrationIdSurveyTarget(ApiSurveys.SurveyService.SurveyServiceClient client) => _client = client;

    public async Task ForwardAsync(MirrorCalibrationIdRecord record, CancellationToken token)
    {
        await _client.CreateSurveyAsync(new ApiSurveys.CreateSurveyRequest { Survey = new ApiSurveys.Survey { Number = record.CorrelationId, SurveyId = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.performance.v1.CalibrationValidationService/NormalizeCalibration and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorCalibrationId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(channel);
        var reply = await source.NormalizeCalibrationAsync(new ApiPerformance.NormalizeCalibrationRequest() { Reason = "main-mirror-calibration-id" }, cancellationToken: token);
        var record = new MirrorCalibrationIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Subject = reply.Calibration.Subject,
        };
        var sinks = new IMirrorCalibrationIdSink[]
        {
            new MirrorCalibrationIdDocumentFolderValidationSink(new ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient(channel)),
            new MirrorCalibrationIdReviewSink(new ApiPerformance.ReviewService.ReviewServiceClient(channel)),
            new MirrorCalibrationIdAuditEventQuerySink(new ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient(channel)),
            new MirrorCalibrationIdSurveyTarget(new ApiSurveys.SurveyService.SurveyServiceClient(channel)),
        };
        await sinks[1].ForwardAsync(record, token);
    }
}
