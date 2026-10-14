using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where CopyFeatureFlagsToken hands each record.</summary>
internal interface ICopyFeatureFlagsTokenWriter
{
    Task WriteAsync(CopyFeatureFlagsTokenRecord record, CancellationToken token);
}

/// <summary>What CopyFeatureFlagsToken keeps of each SuggestFeatureFlags response.</summary>
internal sealed class CopyFeatureFlagsTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.metrics.v1.MetricSeriesQueryService.</summary>
internal sealed class CopyFeatureFlagsTokenMetricSeriesQueryTarget : ICopyFeatureFlagsTokenWriter
{
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _client;

    public CopyFeatureFlagsTokenMetricSeriesQueryTarget(ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient client) => _client = client;

    public async Task WriteAsync(CopyFeatureFlagsTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiMetrics.SuggestMetricSeriesesRequest { OrderBy = record.NextPageToken, Filter = new ApiMetrics.SuggestMetricSeriesesRequest.Types.Filter { Region = record.CorrelationId } }, token, _client.SuggestMetricSeriesesAsync);
    }
}

/// <summary>Sends records to acme.helpdesk.v1.EscalationApprovalService.</summary>
internal sealed class CopyFeatureFlagsTokenEscalationApprovalTarget : ICopyFeatureFlagsTokenWriter
{
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _client;

    public CopyFeatureFlagsTokenEscalationApprovalTarget(ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(CopyFeatureFlagsTokenRecord record, CancellationToken token)
    {
        await _client.ListPendingEscalationsAsync(new ApiHelpdesk.ListPendingEscalationsRequest { Filter = new ApiHelpdesk.ListPendingEscalationsRequest.Types.Filter { Region = record.CorrelationId, Query = record.NextPageToken }, Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.workplaces.v1.BuildingLifecycleService.</summary>
internal sealed class CopyFeatureFlagsTokenBuildingLifecycleTarget : ICopyFeatureFlagsTokenWriter
{
    private readonly ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient _client;

    public CopyFeatureFlagsTokenBuildingLifecycleTarget(ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient client) => _client = client;

    public async Task WriteAsync(CopyFeatureFlagsTokenRecord record, CancellationToken token)
    {
        await _client.SuspendBuildingAsync(new ApiWorkplaces.SuspendBuildingRequest { BuildingId = record.NextPageToken, Details = new ApiWorkplaces.SuspendBuildingRequest.Types.Details { ReasonCode = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.EquipmentLifecycleService.</summary>
internal sealed class CopyFeatureFlagsTokenEquipmentLifecycleWriter : ICopyFeatureFlagsTokenWriter
{
    private readonly ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient _client;

    public CopyFeatureFlagsTokenEquipmentLifecycleWriter(ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient client) => _client = client;

    public async Task WriteAsync(CopyFeatureFlagsTokenRecord record, CancellationToken token)
    {
        await _client.SuspendEquipmentAsync(new ApiOnboarding.SuspendEquipmentRequest { Details = new ApiOnboarding.SuspendEquipmentRequest.Types.Details { ReasonCode = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.featureflags.v1.FeatureFlagQueryService/SuggestFeatureFlags and hands what it keeps to the sink of its kind.</summary>
internal static class CopyFeatureFlagsToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient(channel);
        var found = await source.SuggestFeatureFlagsAsync(new ApiFeatureflags.SuggestFeatureFlagsRequest() { OrderBy = "nightly-copy-feature-flags-token" }, cancellationToken: token);
        var record = new CopyFeatureFlagsTokenRecord
        {
            NextPageToken = found.PageInfo.NextPageToken,
            Status = found.Result.Status,
            CorrelationId = found.Result.CorrelationId,
        };
        var sinks = new ICopyFeatureFlagsTokenWriter[]
        {
            new CopyFeatureFlagsTokenMetricSeriesQueryTarget(new ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient(channel)),
            new CopyFeatureFlagsTokenEscalationApprovalTarget(new ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient(channel)),
            new CopyFeatureFlagsTokenBuildingLifecycleTarget(new ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient(channel)),
            new CopyFeatureFlagsTokenEquipmentLifecycleWriter(new ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient(channel)),
        };
        await sinks[3].WriteAsync(record, token);
    }
}
