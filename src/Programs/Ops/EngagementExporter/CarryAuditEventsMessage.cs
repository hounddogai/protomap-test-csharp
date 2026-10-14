using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where CarryAuditEventsMessage hands each record.</summary>
internal interface ICarryAuditEventsMessageDestination
{
    Task PushAsync(CarryAuditEventsMessageRecord record, CancellationToken token);
}

/// <summary>What CarryAuditEventsMessage keeps of each ResolveAuditEvents response.</summary>
internal sealed class CarryAuditEventsMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Sends records to acme.performance.v1.GoalQueryService.</summary>
internal sealed class CarryAuditEventsMessageGoalQueryWriter : ICarryAuditEventsMessageDestination
{
    private readonly ApiPerformance.GoalQueryService.GoalQueryServiceClient _client;

    public CarryAuditEventsMessageGoalQueryWriter(ApiPerformance.GoalQueryService.GoalQueryServiceClient client) => _client = client;

    public async Task PushAsync(CarryAuditEventsMessageRecord record, CancellationToken token)
    {
        await _client.BrowseGoalsAsync(new ApiPerformance.BrowseGoalsRequest { Filter = new ApiPerformance.BrowseGoalsRequest.Types.Filter { Region = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.RosterWorkflowService.</summary>
internal sealed class CarryAuditEventsMessageRosterWorkflowSink(ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient client) : ICarryAuditEventsMessageDestination
{
    public async Task PushAsync(CarryAuditEventsMessageRecord record, CancellationToken token)
    {
        await client.PauseRosterWorkflowAsync(new ApiScheduling.PauseRosterWorkflowRequest { Roster = new ApiScheduling.Roster { Number = record.NextPageToken }, Details = new ApiScheduling.PauseRosterWorkflowRequest.Types.Details { Step = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.insurance.v1.PolicyQueryService.</summary>
internal sealed class CarryAuditEventsMessagePolicyQueryTarget(ApiInsurance.PolicyQueryService.PolicyQueryServiceClient client) : ICarryAuditEventsMessageDestination
{
    public async Task PushAsync(CarryAuditEventsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiInsurance.CountPoliciesRequest { Filter = new ApiInsurance.CountPoliciesRequest.Types.Filter { OwnerId = record.Status } }, token, client.CountPoliciesAsync);
    }
}

/// <summary>Reads acme.audit.v1.AuditEventQueryService/ResolveAuditEvents and hands what it keeps to the sink of its kind.</summary>
internal static class CarryAuditEventsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient(channel);
        var got = await source.ResolveAuditEventsAsync(new ApiAudit.ResolveAuditEventsRequest(), cancellationToken: token);
        var record = new CarryAuditEventsMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = got.Result.CorrelationId,
            Status = Text.Normalize(got.Result.Status),
            NextPageToken = got.PageInfo.NextPageToken,
        };
        var sinks = new ICarryAuditEventsMessageDestination[]
        {
            new CarryAuditEventsMessageGoalQueryWriter(new ApiPerformance.GoalQueryService.GoalQueryServiceClient(channel)),
            new CarryAuditEventsMessageRosterWorkflowSink(new ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient(channel)),
            new CarryAuditEventsMessagePolicyQueryTarget(new ApiInsurance.PolicyQueryService.PolicyQueryServiceClient(channel)),
        };
        await sinks[1].PushAsync(record, token);
    }
}
