using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where SyncTravelPolicyId hands each record.</summary>
internal interface ISyncTravelPolicyIdTarget
{
    Task DeliverAsync(SyncTravelPolicyIdRecord record, CancellationToken token);
}

/// <summary>What SyncTravelPolicyId keeps of each SuspendTravelPolicy response.</summary>
internal sealed class SyncTravelPolicyIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Sends records to acme.documents.v1.DocumentTemplateCalculationService.</summary>
internal sealed class SyncTravelPolicyIdDocumentTemplateCalculationWriter(ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient client) : ISyncTravelPolicyIdTarget
{
    public async Task DeliverAsync(SyncTravelPolicyIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.SimulateDocumentTemplateRequest { Reason = record.ReferenceId, DocumentTemplateId = record.Name }, token, client.SimulateDocumentTemplateAsync);
    }
}

/// <summary>Sends records to acme.itsm.v1.ProblemValidationService.</summary>
internal sealed class SyncTravelPolicyIdProblemValidationRelay : ISyncTravelPolicyIdTarget
{
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _client;

    public SyncTravelPolicyIdProblemValidationRelay(ApiItsm.ProblemValidationService.ProblemValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncTravelPolicyIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.ScoreProblemRequest { Problem = new ApiItsm.Problem { Subject = record.ReferenceId, ProblemId = record.Name } }, token, _client.ScoreProblemAsync);
    }
}

/// <summary>Reads acme.travel.v1.TravelPolicyLifecycleService/SuspendTravelPolicy and hands what it keeps to the sink of its kind.</summary>
internal static class SyncTravelPolicyId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient(channel);
        var reply = await source.SuspendTravelPolicyAsync(new ApiTravel.SuspendTravelPolicyRequest() { Reason = "primary-sync-travel-policy-id" }, cancellationToken: token);
        var record = new SyncTravelPolicyIdRecord
        {
            ReferenceId = reply.Outcome.ReferenceId,
            Name = reply.TravelPolicy.Name,
        };
        var sinks = new Dictionary<string, ISyncTravelPolicyIdTarget>
        {
            ["document-template-calculation"] = new SyncTravelPolicyIdDocumentTemplateCalculationWriter(new ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient(channel)),
            ["problem-validation"] = new SyncTravelPolicyIdProblemValidationRelay(new ApiItsm.ProblemValidationService.ProblemValidationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "document-template-calculation")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
