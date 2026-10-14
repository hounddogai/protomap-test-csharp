using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where SyncDependentStatus hands each record.</summary>
internal interface ISyncDependentStatusSink
{
    Task SendAsync(SyncDependentStatusRecord record, CancellationToken token);
}

/// <summary>What SyncDependentStatus keeps of each ValidateDependent response.</summary>
internal sealed class SyncDependentStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string DependentId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.scheduling.v1.ShiftWorkflowService.</summary>
internal sealed class SyncDependentStatusShiftWorkflowWriter : ISyncDependentStatusSink
{
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _client;

    public SyncDependentStatusShiftWorkflowWriter(ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(SyncDependentStatusRecord record, CancellationToken token)
    {
        await _client.AdvanceShiftWorkflowAsync(new ApiScheduling.AdvanceShiftWorkflowRequest { ShiftId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.itsm.v1.IncidentApprovalService.</summary>
internal sealed class SyncDependentStatusIncidentApprovalRelay : ISyncDependentStatusSink
{
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _client;

    public SyncDependentStatusIncidentApprovalRelay(ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient client) => _client = client;

    public async Task SendAsync(SyncDependentStatusRecord record, CancellationToken token)
    {
        await _client.DelegateIncidentApprovalAsync(new ApiItsm.DelegateIncidentApprovalRequest { Details = new ApiItsm.DelegateIncidentApprovalRequest.Types.Details { ApproverEmail = record.Message }, Incident = new ApiItsm.Incident { IncidentId = record.DependentId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningService.</summary>
internal sealed class SyncDependentStatusJobOpeningSink(ApiRecruiting.JobOpeningService.JobOpeningServiceClient client) : ISyncDependentStatusSink
{
    public async Task SendAsync(SyncDependentStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.SearchJobOpeningsRequest { Filter = new ApiRecruiting.SearchJobOpeningsRequest.Types.Filter { Region = record.Status } }, token, client.SearchJobOpeningsAsync);
    }
}

/// <summary>Reads acme.benefits.v1.DependentValidationService/ValidateDependent and hands what it keeps to the sink of its kind.</summary>
internal static class SyncDependentStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.DependentValidationService.DependentValidationServiceClient(channel);
        var response = await source.ValidateDependentAsync(new ApiBenefits.ValidateDependentRequest() { Reason = "main-sync-dependent-status" }, cancellationToken: token);
        var record = new SyncDependentStatusRecord
        {
            Status = Text.Normalize(response.Result.Status),
            Message = Text.Normalize(response.Outcome.Message),
            DependentId = response.Dependent.DependentId,
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
        };
        var sinks = new List<ISyncDependentStatusSink>
        {
            new SyncDependentStatusShiftWorkflowWriter(new ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient(channel)),
            new SyncDependentStatusIncidentApprovalRelay(new ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient(channel)),
            new SyncDependentStatusJobOpeningSink(new ApiRecruiting.JobOpeningService.JobOpeningServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.SendAsync(record, token);
    }
}
