using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where RelayOrgUnitWorkflowId hands each record.</summary>
internal interface IRelayOrgUnitWorkflowIdWriter
{
    Task ForwardAsync(RelayOrgUnitWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What RelayOrgUnitWorkflowId keeps of each PauseOrgUnitWorkflow response.</summary>
internal sealed class RelayOrgUnitWorkflowIdRecord
{
    public string OrgUnitId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.performance.v1.FeedbackCalculationService.</summary>
internal sealed class RelayOrgUnitWorkflowIdFeedbackCalculationTarget(ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient client) : IRelayOrgUnitWorkflowIdWriter
{
    public async Task ForwardAsync(RelayOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await client.GetFeedbackRatesAsync(new ApiPerformance.GetFeedbackRatesRequest { FeedbackId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.RosterQueryService.</summary>
internal sealed class RelayOrgUnitWorkflowIdRosterQueryRelay(ApiScheduling.RosterQueryService.RosterQueryServiceClient client) : IRelayOrgUnitWorkflowIdWriter
{
    public async Task ForwardAsync(RelayOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await client.FindRostersAsync(new ApiScheduling.FindRostersRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiScheduling.FindRostersRequest.Types.Filter { Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.licensing.v1.SoftwareLicenseApprovalService.</summary>
internal sealed class RelayOrgUnitWorkflowIdSoftwareLicenseApprovalRelay : IRelayOrgUnitWorkflowIdWriter
{
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _client;

    public RelayOrgUnitWorkflowIdSoftwareLicenseApprovalRelay(ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(RelayOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.ListPendingSoftwareLicensesRequest { Filter = new ApiLicensing.ListPendingSoftwareLicensesRequest.Types.Filter { Query = record.Status, OwnerId = record.OrgUnitId }, OrderBy = record.Message }, token, _client.ListPendingSoftwareLicensesAsync);
    }
}

/// <summary>Hands records to acme.itsm.v1.IncidentWorkflowService.</summary>
internal sealed class RelayOrgUnitWorkflowIdIncidentWorkflowWriter : IRelayOrgUnitWorkflowIdWriter
{
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _client;

    public RelayOrgUnitWorkflowIdIncidentWorkflowWriter(ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(RelayOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await _client.PauseIncidentWorkflowAsync(new ApiItsm.PauseIncidentWorkflowRequest { Details = new ApiItsm.PauseIncidentWorkflowRequest.Types.Details { Step = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayOrgUnitWorkflowIdLogOnly : IRelayOrgUnitWorkflowIdWriter
{
    public Task ForwardAsync(RelayOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.OrgUnitId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitWorkflowService/PauseOrgUnitWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class RelayOrgUnitWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel);
        var got = await source.PauseOrgUnitWorkflowAsync(new ApiEmployees.PauseOrgUnitWorkflowRequest() { OrgUnitId = "live-relay-org-unit-workflow-id" }, cancellationToken: token);
        var record = new RelayOrgUnitWorkflowIdRecord
        {
            OrgUnitId = got.OrgUnit.OrgUnitId,
            Status = got.Outcome.Status,
            Message = Text.Normalize(got.Result.Message),
        };
        await DeliverRelayOrgUnitWorkflowIdAsync("feedback-calculation", record, channel, token);
    }

    private static async Task DeliverRelayOrgUnitWorkflowIdAsync(string kind, RelayOrgUnitWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRelayOrgUnitWorkflowIdWriter sink = new RelayOrgUnitWorkflowIdLogOnly();
        switch (kind)
        {
            case "feedback-calculation":
                sink = new RelayOrgUnitWorkflowIdFeedbackCalculationTarget(new ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient(channel));
                break;
            case "roster-query":
                sink = new RelayOrgUnitWorkflowIdRosterQueryRelay(new ApiScheduling.RosterQueryService.RosterQueryServiceClient(channel));
                break;
            case "software-license-approval":
                sink = new RelayOrgUnitWorkflowIdSoftwareLicenseApprovalRelay(new ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient(channel));
                break;
            case "incident-workflow":
                sink = new RelayOrgUnitWorkflowIdIncidentWorkflowWriter(new ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
