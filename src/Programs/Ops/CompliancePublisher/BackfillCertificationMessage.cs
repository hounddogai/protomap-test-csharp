using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where BackfillCertificationMessage hands each record.</summary>
internal interface IBackfillCertificationMessageDestination
{
    Task ApplyAsync(BackfillCertificationMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillCertificationMessage keeps of each EscalateCertification response.</summary>
internal sealed class BackfillCertificationMessageRecord
{
    public string Message { get; init; } = "";
    public string Number { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.helpdesk.v1.TicketQueryService.</summary>
internal sealed class BackfillCertificationMessageTicketQuerySink(ApiHelpdesk.TicketQueryService.TicketQueryServiceClient client) : IBackfillCertificationMessageDestination
{
    public async Task ApplyAsync(BackfillCertificationMessageRecord record, CancellationToken token)
    {
        await client.SuggestTicketsAsync(new ApiHelpdesk.SuggestTicketsRequest { Filter = new ApiHelpdesk.SuggestTicketsRequest.Types.Filter { Query = record.CorrelationId, OwnerId = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.corporateevents.v1.CorporateEventReportingService.</summary>
internal sealed class BackfillCertificationMessageCorporateEventReportingRelay(ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient client) : IBackfillCertificationMessageDestination
{
    public async Task ApplyAsync(BackfillCertificationMessageRecord record, CancellationToken token)
    {
        await client.CountCorporateEventTotalsAsync(new ApiCorporateevents.CountCorporateEventTotalsRequest { OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.CertificationApprovalService/EscalateCertification and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillCertificationMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient(channel);
        var found = await source.EscalateCertificationAsync(new ApiLearning.EscalateCertificationRequest() { CertificationId = "current-backfill-certification-message" }, cancellationToken: token);
        var record = new BackfillCertificationMessageRecord
        {
            Message = found.Result.Message,
            Number = found.Certification.Number,
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
        };
        await ActOnBackfillCertificationMessageAsync("corporate-event-reporting", record, channel, token);
    }

    private static async Task ActOnBackfillCertificationMessageAsync(string kind, BackfillCertificationMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["ticket-query"] = () => new BackfillCertificationMessageTicketQuerySink(new ApiHelpdesk.TicketQueryService.TicketQueryServiceClient(channel)).ApplyAsync(record, token),
            ["corporate-event-reporting"] = () => new BackfillCertificationMessageCorporateEventReportingRelay(new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
