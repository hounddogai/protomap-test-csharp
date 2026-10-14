using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeaturestore = Acme.Featurestore.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where ReplayLifeEventApprovalStatusId hands each record.</summary>
internal interface IReplayLifeEventApprovalStatusIdDestination
{
    Task SendAsync(ReplayLifeEventApprovalStatusIdRecord record, CancellationToken token);
}

/// <summary>What ReplayLifeEventApprovalStatusId keeps of each GetLifeEventApprovalStatus response.</summary>
internal sealed class ReplayLifeEventApprovalStatusIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Source { get; init; } = "";
}

/// <summary>Writes records to acme.featurestore.v1.FeatureViewCalculationService.</summary>
internal sealed class ReplayLifeEventApprovalStatusIdFeatureViewCalculationTarget : IReplayLifeEventApprovalStatusIdDestination
{
    private readonly ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient _client;

    public ReplayLifeEventApprovalStatusIdFeatureViewCalculationTarget(ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient client) => _client = client;

    public async Task SendAsync(ReplayLifeEventApprovalStatusIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFeaturestore.ExplainFeatureViewCalculationRequest { FeatureViewId = record.Message }, token, _client.ExplainFeatureViewCalculationAsync);
    }
}

/// <summary>Passes records to acme.employees.v1.EmploymentContractApprovalService.</summary>
internal sealed class ReplayLifeEventApprovalStatusIdEmploymentContractApprovalWriter : IReplayLifeEventApprovalStatusIdDestination
{
    private readonly ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient _client;

    public ReplayLifeEventApprovalStatusIdEmploymentContractApprovalWriter(ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient client) => _client = client;

    public async Task SendAsync(ReplayLifeEventApprovalStatusIdRecord record, CancellationToken token)
    {
        await _client.RecallEmploymentContractAsync(new ApiEmployees.RecallEmploymentContractRequest { EmploymentContract = new ApiEmployees.EmploymentContract { Notes = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.benefits.v1.LifeEventApprovalService/GetLifeEventApprovalStatus and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayLifeEventApprovalStatusId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient(channel);
        var found = await source.GetLifeEventApprovalStatusAsync(new ApiBenefits.GetLifeEventApprovalStatusRequest() { LifeEventId = "live-replay-life-event-approval-status-id" }, cancellationToken: token);
        var record = new ReplayLifeEventApprovalStatusIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Message = found.Result.Message,
            Source = found.LifeEvent.Source,
        };
        var sinks = new IReplayLifeEventApprovalStatusIdDestination[]
        {
            new ReplayLifeEventApprovalStatusIdFeatureViewCalculationTarget(new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(channel)),
            new ReplayLifeEventApprovalStatusIdEmploymentContractApprovalWriter(new ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient(channel)),
        };
        await sinks[0].SendAsync(record, token);
    }
}
