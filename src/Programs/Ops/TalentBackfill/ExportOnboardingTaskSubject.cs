using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where ExportOnboardingTaskSubject hands each record.</summary>
internal interface IExportOnboardingTaskSubjectWriter
{
    Task SendAsync(ExportOnboardingTaskSubjectRecord record, CancellationToken token);
}

/// <summary>What ExportOnboardingTaskSubject keeps of each LookupOnboardingTask response.</summary>
internal sealed class ExportOnboardingTaskSubjectRecord
{
    public string Subject { get; init; } = "";
    public string OnboardingTaskId { get; init; } = "";
}

/// <summary>Writes records to acme.devices.v1.ComplianceScanCalculationService.</summary>
internal sealed class ExportOnboardingTaskSubjectComplianceScanCalculationWriter : IExportOnboardingTaskSubjectWriter
{
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _client;

    public ExportOnboardingTaskSubjectComplianceScanCalculationWriter(ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient client) => _client = client;

    public async Task SendAsync(ExportOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        await _client.CalculateComplianceScanAsync(new ApiDevices.CalculateComplianceScanRequest { Details = new ApiDevices.CalculateComplianceScanRequest.Types.Details { CurrencyCode = record.OnboardingTaskId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.corporateevents.v1.CorporateEventApprovalService.</summary>
internal sealed class ExportOnboardingTaskSubjectCorporateEventApprovalTarget : IExportOnboardingTaskSubjectWriter
{
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _client;

    public ExportOnboardingTaskSubjectCorporateEventApprovalTarget(ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient client) => _client = client;

    public async Task SendAsync(ExportOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        await _client.EscalateCorporateEventAsync(new ApiCorporateevents.EscalateCorporateEventRequest { Reason = record.OnboardingTaskId, CorporateEvent = new ApiCorporateevents.CorporateEvent { CorporateEventId = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.documents.v1.DocumentQueryService.</summary>
internal sealed class ExportOnboardingTaskSubjectDocumentQuerySink : IExportOnboardingTaskSubjectWriter
{
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _client;

    public ExportOnboardingTaskSubjectDocumentQuerySink(ApiDocuments.DocumentQueryService.DocumentQueryServiceClient client) => _client = client;

    public async Task SendAsync(ExportOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.CountDocumentsRequest { Filter = new ApiDocuments.CountDocumentsRequest.Types.Filter { Region = record.Subject, Query = record.OnboardingTaskId } }, token, _client.CountDocumentsAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ExportOnboardingTaskSubjectLogOnly : IExportOnboardingTaskSubjectWriter
{
    public Task SendAsync(ExportOnboardingTaskSubjectRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Subject}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskQueryService/LookupOnboardingTask and hands what it keeps to the sink of its kind.</summary>
internal static class ExportOnboardingTaskSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient(channel);
        var reply = await source.LookupOnboardingTaskAsync(new ApiOnboarding.LookupOnboardingTaskRequest() { OnboardingTaskId = "primary-export-onboarding-task-subject" }, cancellationToken: token);
        var record = new ExportOnboardingTaskSubjectRecord
        {
            Subject = reply.OnboardingTask.Subject,
            OnboardingTaskId = reply.OnboardingTask.OnboardingTaskId,
        };
        await DeliverExportOnboardingTaskSubjectAsync("compliance-scan-calculation", record, channel, token);
    }

    private static async Task DeliverExportOnboardingTaskSubjectAsync(string kind, ExportOnboardingTaskSubjectRecord record, GrpcChannel channel, CancellationToken token)
    {
        IExportOnboardingTaskSubjectWriter sink = new ExportOnboardingTaskSubjectLogOnly();
        switch (kind)
        {
            case "compliance-scan-calculation":
                sink = new ExportOnboardingTaskSubjectComplianceScanCalculationWriter(new ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient(channel));
                break;
            case "corporate-event-approval":
                sink = new ExportOnboardingTaskSubjectCorporateEventApprovalTarget(new ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient(channel));
                break;
            case "document-query":
                sink = new ExportOnboardingTaskSubjectDocumentQuerySink(new ApiDocuments.DocumentQueryService.DocumentQueryServiceClient(channel));
                break;
        }
        await sink.SendAsync(record, token);
    }
}
