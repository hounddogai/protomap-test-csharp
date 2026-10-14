using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where CopyOrgUnitWorkflowId hands each record.</summary>
internal interface ICopyOrgUnitWorkflowIdSink
{
    Task PushAsync(CopyOrgUnitWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What CopyOrgUnitWorkflowId keeps of each CancelOrgUnitWorkflow response.</summary>
internal sealed class CopyOrgUnitWorkflowIdRecord
{
    public string OrgUnitId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Passes records to acme.scheduling.v1.SwapRequestCalculationService.</summary>
internal sealed class CopyOrgUnitWorkflowIdSwapRequestCalculationSink : ICopyOrgUnitWorkflowIdSink
{
    private readonly ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient _client;

    public CopyOrgUnitWorkflowIdSwapRequestCalculationSink(ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient client) => _client = client;

    public async Task PushAsync(CopyOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await _client.EstimateSwapRequestAsync(new ApiScheduling.EstimateSwapRequestRequest { SwapRequest = new ApiScheduling.SwapRequest { Subject = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.scheduling.v1.SwapRequestApprovalService.</summary>
internal sealed class CopyOrgUnitWorkflowIdSwapRequestApprovalSink(ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient client) : ICopyOrgUnitWorkflowIdSink
{
    public async Task PushAsync(CopyOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.RecallSwapRequestRequest { SwapRequest = new ApiScheduling.SwapRequest { Subject = record.OrgUnitId, SwapRequestId = record.CorrelationId } }, token, client.RecallSwapRequestAsync);
    }
}

/// <summary>Sends records to acme.venues.v1.VenueQueryService.</summary>
internal sealed class CopyOrgUnitWorkflowIdVenueQuerySink : ICopyOrgUnitWorkflowIdSink
{
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _client;

    public CopyOrgUnitWorkflowIdVenueQuerySink(ApiVenues.VenueQueryService.VenueQueryServiceClient client) => _client = client;

    public async Task PushAsync(CopyOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await _client.SuggestVenuesAsync(new ApiVenues.SuggestVenuesRequest { OrderBy = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employees.v1.OrgUnitApprovalService.</summary>
internal sealed class CopyOrgUnitWorkflowIdOrgUnitApprovalTarget(ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient client) : ICopyOrgUnitWorkflowIdSink
{
    public async Task PushAsync(CopyOrgUnitWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.EscalateOrgUnitRequest { OrgUnit = new ApiEmployees.OrgUnit { ParentId = record.ReferenceId }, Details = new ApiEmployees.EscalateOrgUnitRequest.Types.Details { Comment = record.CorrelationId } }, token, client.EscalateOrgUnitAsync);
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitWorkflowService/CancelOrgUnitWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class CopyOrgUnitWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel);
        var found = await source.CancelOrgUnitWorkflowAsync(new ApiEmployees.CancelOrgUnitWorkflowRequest() { OrgUnitId = "main-copy-org-unit-workflow-id" }, cancellationToken: token);
        var record = new CopyOrgUnitWorkflowIdRecord
        {
            OrgUnitId = Text.Normalize(found.OrgUnit.OrgUnitId),
            CorrelationId = found.Result.CorrelationId,
            ReferenceId = found.Outcome.ReferenceId,
        };
        await ActOnCopyOrgUnitWorkflowIdAsync("swap-request-approval", record, channel, token);
    }

    private static async Task ActOnCopyOrgUnitWorkflowIdAsync(string kind, CopyOrgUnitWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["swap-request-calculation"] = () => new CopyOrgUnitWorkflowIdSwapRequestCalculationSink(new ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient(channel)).PushAsync(record, token),
            ["swap-request-approval"] = () => new CopyOrgUnitWorkflowIdSwapRequestApprovalSink(new ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient(channel)).PushAsync(record, token),
            ["venue-query"] = () => new CopyOrgUnitWorkflowIdVenueQuerySink(new ApiVenues.VenueQueryService.VenueQueryServiceClient(channel)).PushAsync(record, token),
            ["org-unit-approval"] = () => new CopyOrgUnitWorkflowIdOrgUnitApprovalTarget(new ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
