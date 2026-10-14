using Grpc.Net.Client;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiPricing = Acme.Pricing.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where ExportPoliciesTimezone hands each record.</summary>
internal interface IExportPoliciesTimezoneSink
{
    Task WriteAsync(ExportPoliciesTimezoneRecord record, CancellationToken token);
}

/// <summary>What ExportPoliciesTimezone keeps of each SummarizePolicies response.</summary>
internal sealed class ExportPoliciesTimezoneRecord
{
    public string Timezone { get; init; } = "";
    public string Status { get; init; } = "";
    public string DownloadUri { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.timekeeping.v1.ClockEventQueryService.</summary>
internal sealed class ExportPoliciesTimezoneClockEventQueryTarget(ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient client) : IExportPoliciesTimezoneSink
{
    public async Task WriteAsync(ExportPoliciesTimezoneRecord record, CancellationToken token)
    {
        await client.SummarizeClockEventsAsync(new ApiTimekeeping.SummarizeClockEventsRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.pricing.v1.PriceListQueryService.</summary>
internal sealed class ExportPoliciesTimezonePriceListQuerySink(ApiPricing.PriceListQueryService.PriceListQueryServiceClient client) : IExportPoliciesTimezoneSink
{
    public async Task WriteAsync(ExportPoliciesTimezoneRecord record, CancellationToken token)
    {
        await client.CountPriceListsAsync(new ApiPricing.CountPriceListsRequest { OrderBy = record.Status, Filter = new ApiPricing.CountPriceListsRequest.Types.Filter { Query = record.DownloadUri } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.workplaces.v1.BuildingApprovalService.</summary>
internal sealed class ExportPoliciesTimezoneBuildingApprovalTarget : IExportPoliciesTimezoneSink
{
    private readonly ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient _client;

    public ExportPoliciesTimezoneBuildingApprovalTarget(ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(ExportPoliciesTimezoneRecord record, CancellationToken token)
    {
        await _client.GetBuildingApprovalStatusAsync(new ApiWorkplaces.GetBuildingApprovalStatusRequest { BuildingId = record.Timezone }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.featureflags.v1.FeatureFlagValidationService.</summary>
internal sealed class ExportPoliciesTimezoneFeatureFlagValidationSink(ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient client) : IExportPoliciesTimezoneSink
{
    public async Task WriteAsync(ExportPoliciesTimezoneRecord record, CancellationToken token)
    {
        await client.LintFeatureFlagsAsync(new ApiFeatureflags.LintFeatureFlagsRequest { FeatureFlag = new ApiFeatureflags.FeatureFlag { Name = record.DownloadUri } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.insurance.v1.PolicyQueryService/SummarizePolicies and hands what it keeps to the sink of its kind.</summary>
internal static class ExportPoliciesTimezone
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiInsurance.PolicyQueryService.PolicyQueryServiceClient(channel);
        var reply = await source.SummarizePoliciesAsync(new ApiInsurance.SummarizePoliciesRequest() { PolicyId = "default-export-policies-timezone" }, cancellationToken: token);
        var record = new ExportPoliciesTimezoneRecord
        {
            Timezone = reply.Period.Timezone,
            Status = reply.Result.Status,
            DownloadUri = reply.DownloadUri,
            Message = reply.Result.Message,
        };
        var sinks = new Dictionary<string, IExportPoliciesTimezoneSink>
        {
            ["clock-event-query"] = new ExportPoliciesTimezoneClockEventQueryTarget(new ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient(channel)),
            ["price-list-query"] = new ExportPoliciesTimezonePriceListQuerySink(new ApiPricing.PriceListQueryService.PriceListQueryServiceClient(channel)),
            ["building-approval"] = new ExportPoliciesTimezoneBuildingApprovalTarget(new ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient(channel)),
            ["feature-flag-validation"] = new ExportPoliciesTimezoneFeatureFlagValidationSink(new ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "feature-flag-validation")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
