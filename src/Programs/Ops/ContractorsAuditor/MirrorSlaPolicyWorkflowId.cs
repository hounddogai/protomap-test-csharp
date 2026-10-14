using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where MirrorSlaPolicyWorkflowId hands each record.</summary>
internal interface IMirrorSlaPolicyWorkflowIdTarget
{
    Task PushAsync(MirrorSlaPolicyWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What MirrorSlaPolicyWorkflowId keeps of each CancelSlaPolicyWorkflow response.</summary>
internal sealed class MirrorSlaPolicyWorkflowIdRecord
{
    public string SlaPolicyId { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Writes records to acme.travel.v1.ItineraryLifecycleService.</summary>
internal sealed class MirrorSlaPolicyWorkflowIdItineraryLifecycleRelay : IMirrorSlaPolicyWorkflowIdTarget
{
    private readonly ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient _client;

    public MirrorSlaPolicyWorkflowIdItineraryLifecycleRelay(ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient client) => _client = client;

    public async Task PushAsync(MirrorSlaPolicyWorkflowIdRecord record, CancellationToken token)
    {
        await _client.TerminateItineraryAsync(new ApiTravel.TerminateItineraryRequest { ItineraryId = record.Name, Details = new ApiTravel.TerminateItineraryRequest.Types.Details { ReasonCode = record.SlaPolicyId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.documents.v1.SignatureHistoryService.</summary>
internal sealed class MirrorSlaPolicyWorkflowIdSignatureHistoryWriter : IMirrorSlaPolicyWorkflowIdTarget
{
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _client;

    public MirrorSlaPolicyWorkflowIdSignatureHistoryWriter(ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient client) => _client = client;

    public async Task PushAsync(MirrorSlaPolicyWorkflowIdRecord record, CancellationToken token)
    {
        await _client.GetSignatureVersionAsync(new ApiDocuments.GetSignatureVersionRequest { SignatureId = record.SlaPolicyId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.onboarding.v1.EquipmentReportingService.</summary>
internal sealed class MirrorSlaPolicyWorkflowIdEquipmentReportingTarget(ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient client) : IMirrorSlaPolicyWorkflowIdTarget
{
    public async Task PushAsync(MirrorSlaPolicyWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.CountEquipmentTotalsRequest { Filter = new ApiOnboarding.CountEquipmentTotalsRequest.Types.Filter { Region = record.SlaPolicyId } }, token, client.CountEquipmentTotalsAsync);
    }
}

/// <summary>Reads acme.helpdesk.v1.SlaPolicyWorkflowService/CancelSlaPolicyWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorSlaPolicyWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient(channel);
        var found = await source.CancelSlaPolicyWorkflowAsync(new ApiHelpdesk.CancelSlaPolicyWorkflowRequest() { SlaPolicyId = "live-mirror-sla-policy-workflow-id" }, cancellationToken: token);
        var record = new MirrorSlaPolicyWorkflowIdRecord
        {
            SlaPolicyId = found.SlaPolicy.SlaPolicyId,
            Name = Text.Normalize(found.SlaPolicy.Name),
        };
        await ActOnMirrorSlaPolicyWorkflowIdAsync("equipment-reporting", record, channel, token);
    }

    private static async Task ActOnMirrorSlaPolicyWorkflowIdAsync(string kind, MirrorSlaPolicyWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["itinerary-lifecycle"] = () => new MirrorSlaPolicyWorkflowIdItineraryLifecycleRelay(new ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient(channel)).PushAsync(record, token),
            ["signature-history"] = () => new MirrorSlaPolicyWorkflowIdSignatureHistoryWriter(new ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient(channel)).PushAsync(record, token),
            ["equipment-reporting"] = () => new MirrorSlaPolicyWorkflowIdEquipmentReportingTarget(new ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
