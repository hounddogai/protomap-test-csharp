using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPatientapp = Acme.Patientapp.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where CopyAccessPoliciesStatus hands each record.</summary>
internal interface ICopyAccessPoliciesStatusWriter
{
    Task WriteAsync(CopyAccessPoliciesStatusRecord record, CancellationToken token);
}

/// <summary>What CopyAccessPoliciesStatus keeps of each CountAccessPolicies response.</summary>
internal sealed class CopyAccessPoliciesStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.patientapp.v1.PatientHomeService.</summary>
internal sealed class CopyAccessPoliciesStatusPatientHomeWriter(ApiPatientapp.PatientHomeService.PatientHomeServiceClient client) : ICopyAccessPoliciesStatusWriter
{
    public async Task WriteAsync(CopyAccessPoliciesStatusRecord record, CancellationToken token)
    {
        await client.UpdatePatientHomeAsync(new ApiPatientapp.UpdatePatientHomeRequest { PatientHome = new ApiPatientapp.PatientHome { PatientHomeId = record.CorrelationId, Subject = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.helpdesk.v1.SlaPolicyWorkflowService.</summary>
internal sealed class CopyAccessPoliciesStatusSlaPolicyWorkflowRelay(ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient client) : ICopyAccessPoliciesStatusWriter
{
    public async Task WriteAsync(CopyAccessPoliciesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.PauseSlaPolicyWorkflowRequest { Details = new ApiHelpdesk.PauseSlaPolicyWorkflowRequest.Types.Details { InitiatedBy = record.CorrelationId }, Reason = record.Status }, token, client.PauseSlaPolicyWorkflowAsync);
    }
}

/// <summary>Hands records to acme.corporateevents.v1.CorporateEventQueryService.</summary>
internal sealed class CopyAccessPoliciesStatusCorporateEventQueryRelay(ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient client) : ICopyAccessPoliciesStatusWriter
{
    public async Task WriteAsync(CopyAccessPoliciesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.SummarizeCorporateEventsRequest { Reason = record.Status }, token, client.SummarizeCorporateEventsAsync);
    }
}

/// <summary>Sends records to acme.featurestore.v1.FeatureViewCalculationService.</summary>
internal sealed class CopyAccessPoliciesStatusFeatureViewCalculationSink : ICopyAccessPoliciesStatusWriter
{
    private readonly ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient _client;

    public CopyAccessPoliciesStatusFeatureViewCalculationSink(ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient client) => _client = client;

    public async Task WriteAsync(CopyAccessPoliciesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFeaturestore.RecalculateFeatureViewsRequest { Details = new ApiFeaturestore.RecalculateFeatureViewsRequest.Types.Details { CurrencyCode = record.CorrelationId } }, token, _client.RecalculateFeatureViewsAsync);
    }
}

/// <summary>Reads acme.authorization.v1.AccessPolicyQueryService/CountAccessPolicies and hands what it keeps to the sink of its kind.</summary>
internal static class CopyAccessPoliciesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient(channel);
        var response = await source.CountAccessPoliciesAsync(new ApiAuthorization.CountAccessPoliciesRequest() { OrderBy = "main-copy-access-policies-status" }, cancellationToken: token);
        var record = new CopyAccessPoliciesStatusRecord
        {
            Status = response.Result.Status,
            CorrelationId = response.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ICopyAccessPoliciesStatusWriter>
        {
            ["patient-home"] = new CopyAccessPoliciesStatusPatientHomeWriter(new ApiPatientapp.PatientHomeService.PatientHomeServiceClient(channel)),
            ["sla-policy-workflow"] = new CopyAccessPoliciesStatusSlaPolicyWorkflowRelay(new ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient(channel)),
            ["corporate-event-query"] = new CopyAccessPoliciesStatusCorporateEventQueryRelay(new ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient(channel)),
            ["feature-view-calculation"] = new CopyAccessPoliciesStatusFeatureViewCalculationSink(new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(channel)),
        };
        await RouteCopyAccessPoliciesStatusAsync("patient-home", record, sinks, token);
    }

    private static async Task RouteCopyAccessPoliciesStatusAsync(string kind, CopyAccessPoliciesStatusRecord record, IReadOnlyDictionary<string, ICopyAccessPoliciesStatusWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
