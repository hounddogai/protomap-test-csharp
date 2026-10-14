using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPatients = Acme.Patients.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where ExportPatientMessage hands each record.</summary>
internal interface IExportPatientMessageDestination
{
    Task ForwardAsync(ExportPatientMessageRecord record, CancellationToken token);
}

/// <summary>What ExportPatientMessage keeps of each EstimatePatient response.</summary>
internal sealed class ExportPatientMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string PatientId { get; init; } = "";
}

/// <summary>Passes records to acme.helpdesk.v1.TicketValidationService.</summary>
internal sealed class ExportPatientMessageTicketValidationSink(ApiHelpdesk.TicketValidationService.TicketValidationServiceClient client) : IExportPatientMessageDestination
{
    public async Task ForwardAsync(ExportPatientMessageRecord record, CancellationToken token)
    {
        await client.PreviewTicketAsync(new ApiHelpdesk.PreviewTicketRequest { Ticket = new ApiHelpdesk.Ticket { Subject = record.CorrelationId }, Details = new ApiHelpdesk.PreviewTicketRequest.Types.Details { Locale = record.PatientId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.search.v1.SearchIndexService.</summary>
internal sealed class ExportPatientMessageSearchIndexTarget(ApiSearch.SearchIndexService.SearchIndexServiceClient client) : IExportPatientMessageDestination
{
    public async Task ForwardAsync(ExportPatientMessageRecord record, CancellationToken token)
    {
        await client.UpdateSearchIndexAsync(new ApiSearch.UpdateSearchIndexRequest { SearchIndex = new ApiSearch.SearchIndex { Code = record.CurrencyCode, Name = record.PatientId, SearchIndexId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.patients.v1.PatientCalculationService/EstimatePatient and hands what it keeps to the sink of its kind.</summary>
internal static class ExportPatientMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPatients.PatientCalculationService.PatientCalculationServiceClient(channel);
        var got = await source.EstimatePatientAsync(new ApiPatients.EstimatePatientRequest() { PatientId = "current-export-patient-message" }, cancellationToken: token);
        var record = new ExportPatientMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = Text.Normalize(got.Result.CorrelationId),
            CurrencyCode = got.Breakdown.CurrencyCode,
            PatientId = got.Patient.PatientId,
        };
        var sinks = new List<IExportPatientMessageDestination>
        {
            new ExportPatientMessageTicketValidationSink(new ApiHelpdesk.TicketValidationService.TicketValidationServiceClient(channel)),
            new ExportPatientMessageSearchIndexTarget(new ApiSearch.SearchIndexService.SearchIndexServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.ForwardAsync(record, token);
    }
}
