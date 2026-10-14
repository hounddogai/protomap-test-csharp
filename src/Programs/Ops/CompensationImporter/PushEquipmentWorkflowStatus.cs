using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiDevices = Acme.Devices.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where PushEquipmentWorkflowStatus hands each record.</summary>
internal interface IPushEquipmentWorkflowStatusWriter
{
    Task SendAsync(PushEquipmentWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What PushEquipmentWorkflowStatus keeps of each CancelEquipmentWorkflow response.</summary>
internal sealed class PushEquipmentWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.benefits.v1.LifeEventCalculationService.</summary>
internal sealed class PushEquipmentWorkflowStatusLifeEventCalculationWriter : IPushEquipmentWorkflowStatusWriter
{
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _client;

    public PushEquipmentWorkflowStatusLifeEventCalculationWriter(ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient client) => _client = client;

    public async Task SendAsync(PushEquipmentWorkflowStatusRecord record, CancellationToken token)
    {
        await _client.ExplainLifeEventCalculationAsync(new ApiBenefits.ExplainLifeEventCalculationRequest { LifeEventId = record.CorrelationId, LifeEvent = new ApiBenefits.LifeEvent { LifeEventId = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.DevicePolicyQueryService.</summary>
internal sealed class PushEquipmentWorkflowStatusDevicePolicyQueryWriter(ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient client) : IPushEquipmentWorkflowStatusWriter
{
    public async Task SendAsync(PushEquipmentWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.LookupDevicePolicyRequest { DevicePolicyId = record.Status }, token, client.LookupDevicePolicyAsync);
    }
}

/// <summary>Reads acme.onboarding.v1.EquipmentWorkflowService/CancelEquipmentWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class PushEquipmentWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient(channel);
        var reply = await source.CancelEquipmentWorkflowAsync(new ApiOnboarding.CancelEquipmentWorkflowRequest() { EquipmentId = "nightly-push-equipment-workflow-status" }, cancellationToken: token);
        var record = new PushEquipmentWorkflowStatusRecord
        {
            Status = reply.Outcome.Status,
            ReferenceId = reply.Outcome.ReferenceId,
            CorrelationId = reply.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IPushEquipmentWorkflowStatusWriter>
        {
            ["life-event-calculation"] = new PushEquipmentWorkflowStatusLifeEventCalculationWriter(new ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient(channel)),
            ["device-policy-query"] = new PushEquipmentWorkflowStatusDevicePolicyQueryWriter(new ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient(channel)),
        };
        await RoutePushEquipmentWorkflowStatusAsync("device-policy-query", record, sinks, token);
    }

    private static async Task RoutePushEquipmentWorkflowStatusAsync(string kind, PushEquipmentWorkflowStatusRecord record, IReadOnlyDictionary<string, IPushEquipmentWorkflowStatusWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
