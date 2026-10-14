using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiAdmissions = Acme.Admissions.V1;
using ApiContent = Acme.Content.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where BackfillSlaPolicyWorkflowId hands each record.</summary>
internal interface IBackfillSlaPolicyWorkflowIdTarget
{
    Task ApplyAsync(BackfillSlaPolicyWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What BackfillSlaPolicyWorkflowId keeps of each RetrySlaPolicyWorkflow response.</summary>
internal sealed class BackfillSlaPolicyWorkflowIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
}

/// <summary>Passes records to acme.licensing.v1.SoftwareLicenseCalculationService.</summary>
internal sealed class BackfillSlaPolicyWorkflowIdSoftwareLicenseCalculationWriter : IBackfillSlaPolicyWorkflowIdTarget
{
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _client;

    public BackfillSlaPolicyWorkflowIdSoftwareLicenseCalculationWriter(ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillSlaPolicyWorkflowIdRecord record, CancellationToken token)
    {
        await _client.ExplainSoftwareLicenseCalculationAsync(new ApiLicensing.ExplainSoftwareLicenseCalculationRequest { Reason = record.OutcomeStatus, SoftwareLicense = new ApiLicensing.SoftwareLicense { SoftwareLicenseId = record.Status, CurrencyCode = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.admissions.v1.AdmissionService.</summary>
internal sealed class BackfillSlaPolicyWorkflowIdAdmissionSink(ApiAdmissions.AdmissionService.AdmissionServiceClient client) : IBackfillSlaPolicyWorkflowIdTarget
{
    public async Task ApplyAsync(BackfillSlaPolicyWorkflowIdRecord record, CancellationToken token)
    {
        await client.GetAdmissionAsync(new ApiAdmissions.GetAdmissionRequest { AdmissionId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.content.v1.ArticleQueryService.</summary>
internal sealed class BackfillSlaPolicyWorkflowIdArticleQueryRelay : IBackfillSlaPolicyWorkflowIdTarget
{
    private readonly ApiContent.ArticleQueryService.ArticleQueryServiceClient _client;

    public BackfillSlaPolicyWorkflowIdArticleQueryRelay(ApiContent.ArticleQueryService.ArticleQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillSlaPolicyWorkflowIdRecord record, CancellationToken token)
    {
        await _client.SummarizeArticlesAsync(new ApiContent.SummarizeArticlesRequest { Details = new ApiContent.SummarizeArticlesRequest.Types.Details { Format = record.CorrelationId }, ArticleId = record.OutcomeStatus, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillSlaPolicyWorkflowIdLogOnly : IBackfillSlaPolicyWorkflowIdTarget
{
    public Task ApplyAsync(BackfillSlaPolicyWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.SlaPolicyWorkflowService/RetrySlaPolicyWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillSlaPolicyWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient(channel);
        var found = await source.RetrySlaPolicyWorkflowAsync(new ApiHelpdesk.RetrySlaPolicyWorkflowRequest() { SlaPolicyId = "main-backfill-sla-policy-workflow-id" }, cancellationToken: token);
        var record = new BackfillSlaPolicyWorkflowIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Status = found.Result.Status,
            OutcomeStatus = Text.Normalize(found.Outcome.Status),
        };
        await DispatchBackfillSlaPolicyWorkflowIdAsync("software-license-calculation", record, channel, token);
    }

    private static async Task DispatchBackfillSlaPolicyWorkflowIdAsync(string kind, BackfillSlaPolicyWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<BackfillSlaPolicyWorkflowIdRecord, CancellationToken, Task> write = new BackfillSlaPolicyWorkflowIdLogOnly().ApplyAsync;
        switch (kind)
        {
            case "software-license-calculation":
                write = new BackfillSlaPolicyWorkflowIdSoftwareLicenseCalculationWriter(new ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient(channel)).ApplyAsync;
                break;
            case "admission":
                write = new BackfillSlaPolicyWorkflowIdAdmissionSink(new ApiAdmissions.AdmissionService.AdmissionServiceClient(channel)).ApplyAsync;
                break;
            case "article-query":
                write = new BackfillSlaPolicyWorkflowIdArticleQueryRelay(new ApiContent.ArticleQueryService.ArticleQueryServiceClient(channel)).ApplyAsync;
                break;
        }
        await write(record, token);
    }
}
