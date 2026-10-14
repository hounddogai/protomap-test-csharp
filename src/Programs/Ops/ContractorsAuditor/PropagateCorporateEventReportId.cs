using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAdmissions = Acme.Admissions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where PropagateCorporateEventReportId hands each record.</summary>
internal interface IPropagateCorporateEventReportIdSink
{
    Task SendAsync(PropagateCorporateEventReportIdRecord record, CancellationToken token);
}

/// <summary>What PropagateCorporateEventReportId keeps of each ShareCorporateEventReport response.</summary>
internal sealed class PropagateCorporateEventReportIdRecord
{
    public string CorporateEventId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.invoicing.v1.InvoiceBatchCalculationService.</summary>
internal sealed class PropagateCorporateEventReportIdInvoiceBatchCalculationRelay(ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient client) : IPropagateCorporateEventReportIdSink
{
    public async Task SendAsync(PropagateCorporateEventReportIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiInvoicing.GetInvoiceBatchRatesRequest { InvoiceBatchId = record.Status }, token, client.GetInvoiceBatchRatesAsync);
    }
}

/// <summary>Passes records to acme.admissions.v1.AdmissionService.</summary>
internal sealed class PropagateCorporateEventReportIdAdmissionSink(ApiAdmissions.AdmissionService.AdmissionServiceClient client) : IPropagateCorporateEventReportIdSink
{
    public async Task SendAsync(PropagateCorporateEventReportIdRecord record, CancellationToken token)
    {
        await client.CreateAdmissionAsync(new ApiAdmissions.CreateAdmissionRequest { Admission = new ApiAdmissions.Admission { CorrelationId = record.Status, Kind = record.CorporateEventId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.TripValidationService.</summary>
internal sealed class PropagateCorporateEventReportIdTripValidationRelay : IPropagateCorporateEventReportIdSink
{
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _client;

    public PropagateCorporateEventReportIdTripValidationRelay(ApiTravel.TripValidationService.TripValidationServiceClient client) => _client = client;

    public async Task SendAsync(PropagateCorporateEventReportIdRecord record, CancellationToken token)
    {
        await _client.ScoreTripAsync(new ApiTravel.ScoreTripRequest { Trip = new ApiTravel.Trip { TripId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.corporateevents.v1.RegistrationLifecycleService.</summary>
internal sealed class PropagateCorporateEventReportIdRegistrationLifecycleWriter : IPropagateCorporateEventReportIdSink
{
    private readonly ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient _client;

    public PropagateCorporateEventReportIdRegistrationLifecycleWriter(ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient client) => _client = client;

    public async Task SendAsync(PropagateCorporateEventReportIdRecord record, CancellationToken token)
    {
        await _client.ActivateRegistrationAsync(new ApiCorporateevents.ActivateRegistrationRequest { Reason = record.CorrelationId, Details = new ApiCorporateevents.ActivateRegistrationRequest.Types.Details { ReasonCode = record.Status }, RegistrationId = record.CorporateEventId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventReportingService/ShareCorporateEventReport and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateCorporateEventReportId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(channel);
        var got = await source.ShareCorporateEventReportAsync(new ApiCorporateevents.ShareCorporateEventReportRequest() { CorporateEventId = "current-propagate-corporate-event-report-id" }, cancellationToken: token);
        var record = new PropagateCorporateEventReportIdRecord
        {
            CorporateEventId = Text.Normalize(got.CorporateEvent.CorporateEventId),
            CorrelationId = got.Result.CorrelationId,
            Status = got.Result.Status,
        };
        var sinks = new Dictionary<string, IPropagateCorporateEventReportIdSink>
        {
            ["invoice-batch-calculation"] = new PropagateCorporateEventReportIdInvoiceBatchCalculationRelay(new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(channel)),
            ["admission"] = new PropagateCorporateEventReportIdAdmissionSink(new ApiAdmissions.AdmissionService.AdmissionServiceClient(channel)),
            ["trip-validation"] = new PropagateCorporateEventReportIdTripValidationRelay(new ApiTravel.TripValidationService.TripValidationServiceClient(channel)),
            ["registration-lifecycle"] = new PropagateCorporateEventReportIdRegistrationLifecycleWriter(new ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("PROPAGATE_CORPORATE_EVENT_REPORT_ID_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].SendAsync(record, token);
    }
}
