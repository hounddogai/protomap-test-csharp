using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ForwardCorporateEventReportId hands each record.</summary>
internal interface IForwardCorporateEventReportIdDestination
{
    Task SendAsync(ForwardCorporateEventReportIdRecord record, CancellationToken token);
}

/// <summary>What ForwardCorporateEventReportId keeps of each FetchCorporateEventReport response.</summary>
internal sealed class ForwardCorporateEventReportIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.benefits.v1.LifeEventValidationService.</summary>
internal sealed class ForwardCorporateEventReportIdLifeEventValidationRelay(ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient client) : IForwardCorporateEventReportIdDestination
{
    public async Task SendAsync(ForwardCorporateEventReportIdRecord record, CancellationToken token)
    {
        await client.ScoreLifeEventAsync(new ApiBenefits.ScoreLifeEventRequest { LifeEvent = new ApiBenefits.LifeEvent { Kind = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.workplaces.v1.DeskQueryService.</summary>
internal sealed class ForwardCorporateEventReportIdDeskQueryWriter : IForwardCorporateEventReportIdDestination
{
    private readonly ApiWorkplaces.DeskQueryService.DeskQueryServiceClient _client;

    public ForwardCorporateEventReportIdDeskQueryWriter(ApiWorkplaces.DeskQueryService.DeskQueryServiceClient client) => _client = client;

    public async Task SendAsync(ForwardCorporateEventReportIdRecord record, CancellationToken token)
    {
        await _client.SummarizeDesksAsync(new ApiWorkplaces.SummarizeDesksRequest { Details = new ApiWorkplaces.SummarizeDesksRequest.Types.Details { Format = record.CorrelationId }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentFolderNotificationService.</summary>
internal sealed class ForwardCorporateEventReportIdDocumentFolderNotificationWriter : IForwardCorporateEventReportIdDestination
{
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _client;

    public ForwardCorporateEventReportIdDocumentFolderNotificationWriter(ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient client) => _client = client;

    public async Task SendAsync(ForwardCorporateEventReportIdRecord record, CancellationToken token)
    {
        await _client.ListDocumentFolderSubscriptionsAsync(new ApiDocuments.ListDocumentFolderSubscriptionsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventReportingService/FetchCorporateEventReport and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardCorporateEventReportId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(channel);
        var response = await source.FetchCorporateEventReportAsync(new ApiCorporateevents.FetchCorporateEventReportRequest() { CorporateEventId = "nightly-forward-corporate-event-report-id" }, cancellationToken: token);
        var record = new ForwardCorporateEventReportIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Status = response.Result.Status,
        };
        var sinks = new Dictionary<string, IForwardCorporateEventReportIdDestination>
        {
            ["life-event-validation"] = new ForwardCorporateEventReportIdLifeEventValidationRelay(new ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient(channel)),
            ["desk-query"] = new ForwardCorporateEventReportIdDeskQueryWriter(new ApiWorkplaces.DeskQueryService.DeskQueryServiceClient(channel)),
            ["document-folder-notification"] = new ForwardCorporateEventReportIdDocumentFolderNotificationWriter(new ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "desk-query")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
