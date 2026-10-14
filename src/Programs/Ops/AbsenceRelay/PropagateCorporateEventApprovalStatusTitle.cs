using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>What PropagateCorporateEventApprovalStatusTitle keeps of each GetCorporateEventApprovalStatus response.</summary>
internal sealed class PropagateCorporateEventApprovalStatusTitleRecord
{
    public string Title { get; init; } = "";
    public string CorporateEventId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.benefits.v1.EnrollmentApprovalService.</summary>
internal sealed class PropagateCorporateEventApprovalStatusTitleEnrollmentApprovalSink
{
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _client;

    public PropagateCorporateEventApprovalStatusTitleEnrollmentApprovalSink(ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateCorporateEventApprovalStatusTitleRecord record, CancellationToken token)
    {
        await _client.GetEnrollmentApprovalStatusAsync(new ApiBenefits.GetEnrollmentApprovalStatusRequest { EnrollmentId = record.Title }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventApprovalService/GetCorporateEventApprovalStatus and hands what it keeps to acme.benefits.v1.EnrollmentApprovalService.</summary>
internal static class PropagateCorporateEventApprovalStatusTitle
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient(channel);
        var response = await source.GetCorporateEventApprovalStatusAsync(new ApiCorporateevents.GetCorporateEventApprovalStatusRequest() { CorporateEventId = "main-propagate-corporate-event-approval-status-title" }, cancellationToken: token);
        var record = new PropagateCorporateEventApprovalStatusTitleRecord
        {
            Title = response.CorporateEvent.Title,
            CorporateEventId = response.CorporateEvent.CorporateEventId,
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
            Message = response.Result.Message,
        };
        await new PropagateCorporateEventApprovalStatusTitleEnrollmentApprovalSink(new ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient(channel)).ApplyAsync(record, token);
    }
}
