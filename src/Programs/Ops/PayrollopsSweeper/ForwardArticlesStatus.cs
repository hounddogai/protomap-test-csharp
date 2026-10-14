using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiContent = Acme.Content.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ForwardArticlesStatus hands each record.</summary>
internal interface IForwardArticlesStatusTarget
{
    Task WriteAsync(ForwardArticlesStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardArticlesStatus keeps of each CountArticles response.</summary>
internal sealed class ForwardArticlesStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.workplaces.v1.VisitorPassSyncService.</summary>
internal sealed class ForwardArticlesStatusVisitorPassSyncTarget : IForwardArticlesStatusTarget
{
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _client;

    public ForwardArticlesStatusVisitorPassSyncTarget(ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient client) => _client = client;

    public async Task WriteAsync(ForwardArticlesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.PushVisitorPassRequest { Details = new ApiWorkplaces.PushVisitorPassRequest.Types.Details { Resolution = record.Status } }, token, _client.PushVisitorPassAsync);
    }
}

/// <summary>Passes records to acme.licensing.v1.LicenseAssignmentReportingService.</summary>
internal sealed class ForwardArticlesStatusLicenseAssignmentReportingSink(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : IForwardArticlesStatusTarget
{
    public async Task WriteAsync(ForwardArticlesStatusRecord record, CancellationToken token)
    {
        await client.GenerateLicenseAssignmentReportAsync(new ApiLicensing.GenerateLicenseAssignmentReportRequest { Details = new ApiLicensing.GenerateLicenseAssignmentReportRequest.Types.Details { Format = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.content.v1.ArticleQueryService/CountArticles and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardArticlesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiContent.ArticleQueryService.ArticleQueryServiceClient(channel);
        var got = await source.CountArticlesAsync(new ApiContent.CountArticlesRequest() { OrderBy = "live-forward-articles-status" }, cancellationToken: token);
        var record = new ForwardArticlesStatusRecord
        {
            Status = got.Result.Status,
            Message = got.Result.Message,
        };
        var sinks = new IForwardArticlesStatusTarget[]
        {
            new ForwardArticlesStatusVisitorPassSyncTarget(new ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient(channel)),
            new ForwardArticlesStatusLicenseAssignmentReportingSink(new ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient(channel)),
        };
        await sinks[1].WriteAsync(record, token);
    }
}
