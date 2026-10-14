using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where SettleEntitlementStatus hands each record.</summary>
internal interface ISettleEntitlementStatusDestination
{
    Task ApplyAsync(SettleEntitlementStatusRecord record, CancellationToken token);
}

/// <summary>What SettleEntitlementStatus keeps of each ExpireEntitlement response.</summary>
internal sealed class SettleEntitlementStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.authorization.v1.AccessPolicyValidationService.</summary>
internal sealed class SettleEntitlementStatusAccessPolicyValidationSink(ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient client) : ISettleEntitlementStatusDestination
{
    public async Task ApplyAsync(SettleEntitlementStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAuthorization.PreviewAccessPolicyRequest { AccessPolicy = new ApiAuthorization.AccessPolicy { Name = record.Status }, Details = new ApiAuthorization.PreviewAccessPolicyRequest.Types.Details { Locale = record.CorrelationId } }, token, client.PreviewAccessPolicyAsync);
    }
}

/// <summary>Sends records to acme.recruiting.v1.JobOpeningCalculationService.</summary>
internal sealed class SettleEntitlementStatusJobOpeningCalculationWriter(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) : ISettleEntitlementStatusDestination
{
    public async Task ApplyAsync(SettleEntitlementStatusRecord record, CancellationToken token)
    {
        await client.SimulateJobOpeningAsync(new ApiRecruiting.SimulateJobOpeningRequest { JobOpeningId = record.CorrelationId, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.lineage.v1.LineageNodeQueryService.</summary>
internal sealed class SettleEntitlementStatusLineageNodeQuerySink : ISettleEntitlementStatusDestination
{
    private readonly ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient _client;

    public SettleEntitlementStatusLineageNodeQuerySink(ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleEntitlementStatusRecord record, CancellationToken token)
    {
        await _client.BrowseLineageNodesAsync(new ApiLineage.BrowseLineageNodesRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiLineage.BrowseLineageNodesRequest.Types.Filter { Region = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.surveys.v1.SurveyQueryService.</summary>
internal sealed class SettleEntitlementStatusSurveyQueryWriter : ISettleEntitlementStatusDestination
{
    private readonly ApiSurveys.SurveyQueryService.SurveyQueryServiceClient _client;

    public SettleEntitlementStatusSurveyQueryWriter(ApiSurveys.SurveyQueryService.SurveyQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(SettleEntitlementStatusRecord record, CancellationToken token)
    {
        await _client.SuggestSurveysAsync(new ApiSurveys.SuggestSurveysRequest { Filter = new ApiSurveys.SuggestSurveysRequest.Types.Filter { Query = record.Status, Region = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.licensing.v1.EntitlementLifecycleService/ExpireEntitlement and hands what it keeps to the sink of its kind.</summary>
internal static class SettleEntitlementStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient(channel);
        var got = await source.ExpireEntitlementAsync(new ApiLicensing.ExpireEntitlementRequest() { EntitlementId = "main-settle-entitlement-status" }, cancellationToken: token);
        var record = new SettleEntitlementStatusRecord
        {
            Status = Text.Normalize(got.Outcome.Status),
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ISettleEntitlementStatusDestination>
        {
            ["access-policy-validation"] = new SettleEntitlementStatusAccessPolicyValidationSink(new ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient(channel)),
            ["job-opening-calculation"] = new SettleEntitlementStatusJobOpeningCalculationWriter(new ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient(channel)),
            ["lineage-node-query"] = new SettleEntitlementStatusLineageNodeQuerySink(new ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient(channel)),
            ["survey-query"] = new SettleEntitlementStatusSurveyQueryWriter(new ApiSurveys.SurveyQueryService.SurveyQueryServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "survey-query")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
