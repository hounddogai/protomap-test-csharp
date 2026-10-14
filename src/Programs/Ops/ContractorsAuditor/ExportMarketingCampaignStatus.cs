using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiCampaigns = Acme.Campaigns.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where ExportMarketingCampaignStatus hands each record.</summary>
internal interface IExportMarketingCampaignStatusWriter
{
    Task WriteAsync(ExportMarketingCampaignStatusRecord record, CancellationToken token);
}

/// <summary>What ExportMarketingCampaignStatus keeps of each DeleteMarketingCampaign response.</summary>
internal sealed class ExportMarketingCampaignStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.benefits.v1.DependentApprovalService.</summary>
internal sealed class ExportMarketingCampaignStatusDependentApprovalWriter : IExportMarketingCampaignStatusWriter
{
    private readonly ApiBenefits.DependentApprovalService.DependentApprovalServiceClient _client;

    public ExportMarketingCampaignStatusDependentApprovalWriter(ApiBenefits.DependentApprovalService.DependentApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(ExportMarketingCampaignStatusRecord record, CancellationToken token)
    {
        await _client.SubmitDependentAsync(new ApiBenefits.SubmitDependentRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskWorkflowService.</summary>
internal sealed class ExportMarketingCampaignStatusOnboardingTaskWorkflowSink : IExportMarketingCampaignStatusWriter
{
    private readonly ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient _client;

    public ExportMarketingCampaignStatusOnboardingTaskWorkflowSink(ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(ExportMarketingCampaignStatusRecord record, CancellationToken token)
    {
        await _client.AdvanceOnboardingTaskWorkflowAsync(new ApiOnboarding.AdvanceOnboardingTaskWorkflowRequest { OnboardingTask = new ApiOnboarding.OnboardingTask { Subject = record.Status, OnboardingTaskId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.licensing.v1.EntitlementWorkflowService.</summary>
internal sealed class ExportMarketingCampaignStatusEntitlementWorkflowWriter : IExportMarketingCampaignStatusWriter
{
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _client;

    public ExportMarketingCampaignStatusEntitlementWorkflowWriter(ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(ExportMarketingCampaignStatusRecord record, CancellationToken token)
    {
        await _client.SignalEntitlementWorkflowAsync(new ApiLicensing.SignalEntitlementWorkflowRequest { Entitlement = new ApiLicensing.Entitlement { Description = record.Status, Name = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.campaigns.v1.MarketingCampaignService/DeleteMarketingCampaign and hands what it keeps to the sink of its kind.</summary>
internal static class ExportMarketingCampaignStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient(channel);
        var reply = await source.DeleteMarketingCampaignAsync(new ApiCampaigns.DeleteMarketingCampaignRequest() { Reason = "live-export-marketing-campaign-status" }, cancellationToken: token);
        var record = new ExportMarketingCampaignStatusRecord
        {
            Status = reply.Result.Status,
            Message = reply.Result.Message,
        };
        var sinks = new List<IExportMarketingCampaignStatusWriter>
        {
            new ExportMarketingCampaignStatusDependentApprovalWriter(new ApiBenefits.DependentApprovalService.DependentApprovalServiceClient(channel)),
            new ExportMarketingCampaignStatusOnboardingTaskWorkflowSink(new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel)),
            new ExportMarketingCampaignStatusEntitlementWorkflowWriter(new ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.WriteAsync(record, token);
    }
}
