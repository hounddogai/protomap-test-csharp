using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiPayroll = Acme.Payroll.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>What PushPayRunNotes keeps of each GetPayRun response.</summary>
internal sealed class PushPayRunNotesRecord
{
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.corporateevents.v1.CorporateEventService.</summary>
internal sealed class PushPayRunNotesCorporateEventWriter(ApiCorporateevents.CorporateEventService.CorporateEventServiceClient client)
{
    public async Task SendAsync(PushPayRunNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.SearchCorporateEventsRequest { Filter = new ApiCorporateevents.SearchCorporateEventsRequest.Types.Filter { OwnerId = record.Status }, OrderBy = record.Notes }, token, client.SearchCorporateEventsAsync);
    }
}

/// <summary>Reads acme.payroll.v1.PayRunService/GetPayRun and hands what it keeps to acme.corporateevents.v1.CorporateEventService.</summary>
internal static class PushPayRunNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPayroll.PayRunService.PayRunServiceClient(channel);
        var response = await source.GetPayRunAsync(new ApiPayroll.GetPayRunRequest() { PayRunId = "nightly-push-pay-run-notes" }, cancellationToken: token);
        var record = new PushPayRunNotesRecord
        {
            Notes = response.PayRun.Notes,
            Status = response.Result.Status,
            CorrelationId = response.Result.CorrelationId,
        };
        await new PushPayRunNotesCorporateEventWriter(new ApiCorporateevents.CorporateEventService.CorporateEventServiceClient(channel)).SendAsync(record, token);
    }
}
