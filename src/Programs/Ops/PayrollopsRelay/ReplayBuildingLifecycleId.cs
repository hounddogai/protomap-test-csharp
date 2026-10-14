using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where ReplayBuildingLifecycleId hands each record.</summary>
internal interface IReplayBuildingLifecycleIdWriter
{
    Task PushAsync(ReplayBuildingLifecycleIdRecord record, CancellationToken token);
}

/// <summary>What ReplayBuildingLifecycleId keeps of each GetBuildingLifecycle response.</summary>
internal sealed class ReplayBuildingLifecycleIdRecord
{
    public string ParentId { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentTemplateHistoryService.</summary>
internal sealed class ReplayBuildingLifecycleIdDocumentTemplateHistoryTarget : IReplayBuildingLifecycleIdWriter
{
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _client;

    public ReplayBuildingLifecycleIdDocumentTemplateHistoryTarget(ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient client) => _client = client;

    public async Task PushAsync(ReplayBuildingLifecycleIdRecord record, CancellationToken token)
    {
        await _client.ListDocumentTemplateVersionsAsync(new ApiDocuments.ListDocumentTemplateVersionsRequest { Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter { Region = record.ParentId }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.EquipmentValidationService.</summary>
internal sealed class ReplayBuildingLifecycleIdEquipmentValidationSink(ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient client) : IReplayBuildingLifecycleIdWriter
{
    public async Task PushAsync(ReplayBuildingLifecycleIdRecord record, CancellationToken token)
    {
        await client.GetEquipmentRulesAsync(new ApiOnboarding.GetEquipmentRulesRequest { EquipmentId = record.ParentId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.BuildingLifecycleService/GetBuildingLifecycle and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayBuildingLifecycleId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient(channel);
        var got = await source.GetBuildingLifecycleAsync(new ApiWorkplaces.GetBuildingLifecycleRequest() { BuildingId = "primary-replay-building-lifecycle-id" }, cancellationToken: token);
        var record = new ReplayBuildingLifecycleIdRecord
        {
            ParentId = got.Building.ParentId,
            Status = Text.Normalize(got.Result.Status),
            CorrelationId = Text.Normalize(got.Result.CorrelationId),
        };
        var sinks = new Dictionary<string, IReplayBuildingLifecycleIdWriter>
        {
            ["document-template-history"] = new ReplayBuildingLifecycleIdDocumentTemplateHistoryTarget(new ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient(channel)),
            ["equipment-validation"] = new ReplayBuildingLifecycleIdEquipmentValidationSink(new ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("REPLAY_BUILDING_LIFECYCLE_ID_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].PushAsync(record, token);
    }
}
