using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where HandOffDocumentTemplatesCode hands each record.</summary>
internal interface IHandOffDocumentTemplatesCodeTarget
{
    Task SendAsync(HandOffDocumentTemplatesCodeRecord record, CancellationToken token);
}

/// <summary>What HandOffDocumentTemplatesCode keeps of each RecalculateDocumentTemplates response.</summary>
internal sealed class HandOffDocumentTemplatesCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.learning.v1.CourseApprovalService.</summary>
internal sealed class HandOffDocumentTemplatesCodeCourseApprovalRelay(ApiLearning.CourseApprovalService.CourseApprovalServiceClient client) : IHandOffDocumentTemplatesCodeTarget
{
    public async Task SendAsync(HandOffDocumentTemplatesCodeRecord record, CancellationToken token)
    {
        await client.ApproveCourseAsync(new ApiLearning.ApproveCourseRequest { CourseId = record.Message, Course = new ApiLearning.Course { Code = record.Status }, Details = new ApiLearning.ApproveCourseRequest.Types.Details { Comment = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentExportService.</summary>
internal sealed class HandOffDocumentTemplatesCodeDocumentExportTarget : IHandOffDocumentTemplatesCodeTarget
{
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _client;

    public HandOffDocumentTemplatesCodeDocumentExportTarget(ApiDocuments.DocumentExportService.DocumentExportServiceClient client) => _client = client;

    public async Task SendAsync(HandOffDocumentTemplatesCodeRecord record, CancellationToken token)
    {
        await _client.CancelDocumentExportAsync(new ApiDocuments.CancelDocumentExportRequest { DocumentId = record.CurrencyCode, Details = new ApiDocuments.CancelDocumentExportRequest.Types.Details { ReasonCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffDocumentTemplatesCodeLogOnly : IHandOffDocumentTemplatesCodeTarget
{
    public Task SendAsync(HandOffDocumentTemplatesCodeRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CurrencyCode}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.DocumentTemplateCalculationService/RecalculateDocumentTemplates and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffDocumentTemplatesCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient(channel);
        var reply = await source.RecalculateDocumentTemplatesAsync(new ApiDocuments.RecalculateDocumentTemplatesRequest() { Reason = "nightly-hand-off-document-templates-code" }, cancellationToken: token);
        var record = new HandOffDocumentTemplatesCodeRecord
        {
            CurrencyCode = Text.Normalize(reply.Breakdown.CurrencyCode),
            CorrelationId = reply.Result.CorrelationId,
            Message = reply.Result.Message,
            Status = Text.Normalize(reply.Result.Status),
        };
        await DispatchHandOffDocumentTemplatesCodeAsync("course-approval", record, channel, token);
    }

    private static async Task DispatchHandOffDocumentTemplatesCodeAsync(string kind, HandOffDocumentTemplatesCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<HandOffDocumentTemplatesCodeRecord, CancellationToken, Task> write = new HandOffDocumentTemplatesCodeLogOnly().SendAsync;
        switch (kind)
        {
            case "course-approval":
                write = new HandOffDocumentTemplatesCodeCourseApprovalRelay(new ApiLearning.CourseApprovalService.CourseApprovalServiceClient(channel)).SendAsync;
                break;
            case "document-export":
                write = new HandOffDocumentTemplatesCodeDocumentExportTarget(new ApiDocuments.DocumentExportService.DocumentExportServiceClient(channel)).SendAsync;
                break;
        }
        await write(record, token);
    }
}
