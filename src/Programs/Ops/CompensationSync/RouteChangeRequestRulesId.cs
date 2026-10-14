using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where RouteChangeRequestRulesId hands each record.</summary>
internal interface IRouteChangeRequestRulesIdSink
{
    Task SendAsync(RouteChangeRequestRulesIdRecord record, CancellationToken token);
}

/// <summary>What RouteChangeRequestRulesId keeps of each GetChangeRequestRules response.</summary>
internal sealed class RouteChangeRequestRulesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Sends records to acme.documents.v1.DocumentTemplateExportService.</summary>
internal sealed class RouteChangeRequestRulesIdDocumentTemplateExportSink : IRouteChangeRequestRulesIdSink
{
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _client;

    public RouteChangeRequestRulesIdDocumentTemplateExportSink(ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient client) => _client = client;

    public async Task SendAsync(RouteChangeRequestRulesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.CancelDocumentTemplateExportRequest { Reason = record.Subject }, token, _client.CancelDocumentTemplateExportAsync);
    }
}

/// <summary>Sends records to acme.benefits.v1.BenefitPlanApprovalService.</summary>
internal sealed class RouteChangeRequestRulesIdBenefitPlanApprovalRelay(ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient client) : IRouteChangeRequestRulesIdSink
{
    public async Task SendAsync(RouteChangeRequestRulesIdRecord record, CancellationToken token)
    {
        await client.RejectBenefitPlanAsync(new ApiBenefits.RejectBenefitPlanRequest { BenefitPlanId = record.Subject, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ChangeRequestValidationService/GetChangeRequestRules and hands what it keeps to the sink of its kind.</summary>
internal static class RouteChangeRequestRulesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient(channel);
        var reply = await source.GetChangeRequestRulesAsync(new ApiItsm.GetChangeRequestRulesRequest() { ChangeRequestId = "primary-route-change-request-rules-id" }, cancellationToken: token);
        var record = new RouteChangeRequestRulesIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
            Subject = reply.ChangeRequest.Subject,
        };
        await ActOnRouteChangeRequestRulesIdAsync("benefit-plan-approval", record, channel, token);
    }

    private static async Task ActOnRouteChangeRequestRulesIdAsync(string kind, RouteChangeRequestRulesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["document-template-export"] = () => new RouteChangeRequestRulesIdDocumentTemplateExportSink(new ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient(channel)).SendAsync(record, token),
            ["benefit-plan-approval"] = () => new RouteChangeRequestRulesIdBenefitPlanApprovalRelay(new ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient(channel)).SendAsync(record, token),
        };
        await actions[kind]();
    }
}
