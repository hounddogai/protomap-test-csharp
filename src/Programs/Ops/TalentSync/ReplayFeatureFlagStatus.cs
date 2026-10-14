using Grpc.Net.Client;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where ReplayFeatureFlagStatus hands each record.</summary>
internal interface IReplayFeatureFlagStatusWriter
{
    Task ForwardAsync(ReplayFeatureFlagStatusRecord record, CancellationToken token);
}

/// <summary>What ReplayFeatureFlagStatus keeps of each ValidateFeatureFlag response.</summary>
internal sealed class ReplayFeatureFlagStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningValidationService.</summary>
internal sealed class ReplayFeatureFlagStatusJobOpeningValidationRelay : IReplayFeatureFlagStatusWriter
{
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _client;

    public ReplayFeatureFlagStatusJobOpeningValidationRelay(ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayFeatureFlagStatusRecord record, CancellationToken token)
    {
        await _client.ExplainJobOpeningDecisionAsync(new ApiRecruiting.ExplainJobOpeningDecisionRequest { JobOpening = new ApiRecruiting.JobOpening { JobOpeningId = record.CorrelationId }, Details = new ApiRecruiting.ExplainJobOpeningDecisionRequest.Types.Details { Locale = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.workplaces.v1.BuildingService.</summary>
internal sealed class ReplayFeatureFlagStatusBuildingWriter : IReplayFeatureFlagStatusWriter
{
    private readonly ApiWorkplaces.BuildingService.BuildingServiceClient _client;

    public ReplayFeatureFlagStatusBuildingWriter(ApiWorkplaces.BuildingService.BuildingServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayFeatureFlagStatusRecord record, CancellationToken token)
    {
        await _client.GetBuildingAsync(new ApiWorkplaces.GetBuildingRequest { BuildingId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.employees.v1.OrgUnitWorkflowService.</summary>
internal sealed class ReplayFeatureFlagStatusOrgUnitWorkflowWriter(ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient client) : IReplayFeatureFlagStatusWriter
{
    public async Task ForwardAsync(ReplayFeatureFlagStatusRecord record, CancellationToken token)
    {
        await client.AdvanceOrgUnitWorkflowAsync(new ApiEmployees.AdvanceOrgUnitWorkflowRequest { OrgUnit = new ApiEmployees.OrgUnit { OrgUnitId = record.CorrelationId, ParentId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.documents.v1.DocumentSyncService.</summary>
internal sealed class ReplayFeatureFlagStatusDocumentSyncSink : IReplayFeatureFlagStatusWriter
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _client;

    public ReplayFeatureFlagStatusDocumentSyncSink(ApiDocuments.DocumentSyncService.DocumentSyncServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayFeatureFlagStatusRecord record, CancellationToken token)
    {
        await _client.PullDocumentsAsync(new ApiDocuments.PullDocumentsRequest { Document = new ApiDocuments.Document { Number = record.CorrelationId }, DocumentId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.featureflags.v1.FeatureFlagValidationService/ValidateFeatureFlag and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayFeatureFlagStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient(channel);
        var got = await source.ValidateFeatureFlagAsync(new ApiFeatureflags.ValidateFeatureFlagRequest() { FeatureFlagId = "main-replay-feature-flag-status" }, cancellationToken: token);
        var record = new ReplayFeatureFlagStatusRecord
        {
            Status = got.Outcome.Status,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new List<IReplayFeatureFlagStatusWriter>
        {
            new ReplayFeatureFlagStatusJobOpeningValidationRelay(new ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient(channel)),
            new ReplayFeatureFlagStatusBuildingWriter(new ApiWorkplaces.BuildingService.BuildingServiceClient(channel)),
            new ReplayFeatureFlagStatusOrgUnitWorkflowWriter(new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel)),
            new ReplayFeatureFlagStatusDocumentSyncSink(new ApiDocuments.DocumentSyncService.DocumentSyncServiceClient(channel)),
        };
        await sinks[1].ForwardAsync(record, token);
    }
}
