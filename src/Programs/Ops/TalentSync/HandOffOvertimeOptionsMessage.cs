using Grpc.Net.Client;
using ApiAudit = Acme.Audit.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where HandOffOvertimeOptionsMessage hands each record.</summary>
internal interface IHandOffOvertimeOptionsMessageSink
{
    Task SendAsync(HandOffOvertimeOptionsMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffOvertimeOptionsMessage keeps of each CompareOvertimeOptions response.</summary>
internal sealed class HandOffOvertimeOptionsMessageRecord
{
    public string Message { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string Description { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.audit.v1.AuditEventValidationService.</summary>
internal sealed class HandOffOvertimeOptionsMessageAuditEventValidationRelay(ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient client) : IHandOffOvertimeOptionsMessageSink
{
    public async Task SendAsync(HandOffOvertimeOptionsMessageRecord record, CancellationToken token)
    {
        await client.GetAuditEventRulesAsync(new ApiAudit.GetAuditEventRulesRequest { AuditEventId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentSyncService.</summary>
internal sealed class HandOffOvertimeOptionsMessageDocumentSyncTarget(ApiDocuments.DocumentSyncService.DocumentSyncServiceClient client) : IHandOffOvertimeOptionsMessageSink
{
    public async Task SendAsync(HandOffOvertimeOptionsMessageRecord record, CancellationToken token)
    {
        await client.ResolveDocumentConflictAsync(new ApiDocuments.ResolveDocumentConflictRequest { Reason = record.CurrencyCode, Details = new ApiDocuments.ResolveDocumentConflictRequest.Types.Details { PeerRegion = record.Description, Checksum = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.corporateevents.v1.CorporateEventQueryService.</summary>
internal sealed class HandOffOvertimeOptionsMessageCorporateEventQueryRelay(ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient client) : IHandOffOvertimeOptionsMessageSink
{
    public async Task SendAsync(HandOffOvertimeOptionsMessageRecord record, CancellationToken token)
    {
        await client.SummarizeCorporateEventsAsync(new ApiCorporateevents.SummarizeCorporateEventsRequest { CorporateEventId = record.CorrelationId, Details = new ApiCorporateevents.SummarizeCorporateEventsRequest.Types.Details { Cron = record.Description }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.timekeeping.v1.OvertimeCalculationService/CompareOvertimeOptions and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffOvertimeOptionsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(channel);
        var got = await source.CompareOvertimeOptionsAsync(new ApiTimekeeping.CompareOvertimeOptionsRequest() { OvertimeId = "primary-hand-off-overtime-options-message" }, cancellationToken: token);
        var record = new HandOffOvertimeOptionsMessageRecord
        {
            Message = got.Result.Message,
            CurrencyCode = got.Breakdown.CurrencyCode,
            Description = got.Overtime.Description,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new IHandOffOvertimeOptionsMessageSink[]
        {
            new HandOffOvertimeOptionsMessageAuditEventValidationRelay(new ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient(channel)),
            new HandOffOvertimeOptionsMessageDocumentSyncTarget(new ApiDocuments.DocumentSyncService.DocumentSyncServiceClient(channel)),
            new HandOffOvertimeOptionsMessageCorporateEventQueryRelay(new ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient(channel)),
        };
        await sinks[0].SendAsync(record, token);
    }
}
