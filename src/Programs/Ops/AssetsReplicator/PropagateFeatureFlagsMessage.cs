using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPatientapp = Acme.Patientapp.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where PropagateFeatureFlagsMessage hands each record.</summary>
internal interface IPropagateFeatureFlagsMessageTarget
{
    Task PushAsync(PropagateFeatureFlagsMessageRecord record, CancellationToken token);
}

/// <summary>What PropagateFeatureFlagsMessage keeps of each ListFeatureFlags response.</summary>
internal sealed class PropagateFeatureFlagsMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Passes records to acme.boxoffice.v1.BoxOfficeSaleQueryService.</summary>
internal sealed class PropagateFeatureFlagsMessageBoxOfficeSaleQuerySink : IPropagateFeatureFlagsMessageTarget
{
    private readonly ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient _client;

    public PropagateFeatureFlagsMessageBoxOfficeSaleQuerySink(ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient client) => _client = client;

    public async Task PushAsync(PropagateFeatureFlagsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBoxoffice.BrowseBoxOfficeSalesRequest { Filter = new ApiBoxoffice.BrowseBoxOfficeSalesRequest.Types.Filter { OwnerId = record.CorrelationId }, OrderBy = record.NextPageToken }, token, _client.BrowseBoxOfficeSalesAsync);
    }
}

/// <summary>Hands records to acme.patientapp.v1.PatientHomeService.</summary>
internal sealed class PropagateFeatureFlagsMessagePatientHomeTarget : IPropagateFeatureFlagsMessageTarget
{
    private readonly ApiPatientapp.PatientHomeService.PatientHomeServiceClient _client;

    public PropagateFeatureFlagsMessagePatientHomeTarget(ApiPatientapp.PatientHomeService.PatientHomeServiceClient client) => _client = client;

    public async Task PushAsync(PropagateFeatureFlagsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPatientapp.GetPatientHomeRequest { PatientHomeId = record.NextPageToken }, token, _client.GetPatientHomeAsync);
    }
}

/// <summary>Sends records to acme.helpdesk.v1.EscalationApprovalService.</summary>
internal sealed class PropagateFeatureFlagsMessageEscalationApprovalRelay : IPropagateFeatureFlagsMessageTarget
{
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _client;

    public PropagateFeatureFlagsMessageEscalationApprovalRelay(ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient client) => _client = client;

    public async Task PushAsync(PropagateFeatureFlagsMessageRecord record, CancellationToken token)
    {
        await _client.CommentEscalationApprovalAsync(new ApiHelpdesk.CommentEscalationApprovalRequest { Escalation = new ApiHelpdesk.Escalation { Subject = record.Message }, EscalationId = record.CorrelationId, Details = new ApiHelpdesk.CommentEscalationApprovalRequest.Types.Details { ApproverEmail = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.featureflags.v1.FeatureFlagService/ListFeatureFlags and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateFeatureFlagsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient(channel);
        var response = await source.ListFeatureFlagsAsync(new ApiFeatureflags.ListFeatureFlagsRequest() { OrderBy = "primary-propagate-feature-flags-message" }, cancellationToken: token);
        var record = new PropagateFeatureFlagsMessageRecord
        {
            Message = Text.Normalize(response.Result.Message),
            CorrelationId = response.Result.CorrelationId,
            NextPageToken = response.PageInfo.NextPageToken,
        };
        var sinks = new Dictionary<string, IPropagateFeatureFlagsMessageTarget>
        {
            ["box-office-sale-query"] = new PropagateFeatureFlagsMessageBoxOfficeSaleQuerySink(new ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient(channel)),
            ["patient-home"] = new PropagateFeatureFlagsMessagePatientHomeTarget(new ApiPatientapp.PatientHomeService.PatientHomeServiceClient(channel)),
            ["escalation-approval"] = new PropagateFeatureFlagsMessageEscalationApprovalRelay(new ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient(channel)),
        };
        await RoutePropagateFeatureFlagsMessageAsync("box-office-sale-query", record, sinks, token);
    }

    private static async Task RoutePropagateFeatureFlagsMessageAsync(string kind, PropagateFeatureFlagsMessageRecord record, IReadOnlyDictionary<string, IPropagateFeatureFlagsMessageTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.PushAsync(record, token);
    }
}
