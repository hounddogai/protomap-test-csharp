using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where RouteDocumentFoldersStatus hands each record.</summary>
internal interface IRouteDocumentFoldersStatusDestination
{
    Task DeliverAsync(RouteDocumentFoldersStatusRecord record, CancellationToken token);
}

/// <summary>What RouteDocumentFoldersStatus keeps of each FindDocumentFolders response.</summary>
internal sealed class RouteDocumentFoldersStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.timekeeping.v1.ClockEventWorkflowService.</summary>
internal sealed class RouteDocumentFoldersStatusClockEventWorkflowTarget : IRouteDocumentFoldersStatusDestination
{
    private readonly ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient _client;

    public RouteDocumentFoldersStatusClockEventWorkflowTarget(ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(RouteDocumentFoldersStatusRecord record, CancellationToken token)
    {
        await _client.PauseClockEventWorkflowAsync(new ApiTimekeeping.PauseClockEventWorkflowRequest { Details = new ApiTimekeeping.PauseClockEventWorkflowRequest.Types.Details { Step = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employeeportal.v1.SelfServiceRequestIntegrationService.</summary>
internal sealed class RouteDocumentFoldersStatusSelfServiceRequestIntegrationRelay(ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient client) : IRouteDocumentFoldersStatusDestination
{
    public async Task DeliverAsync(RouteDocumentFoldersStatusRecord record, CancellationToken token)
    {
        await client.TestSelfServiceRequestConnectionAsync(new ApiEmployeeportal.TestSelfServiceRequestConnectionRequest { Details = new ApiEmployeeportal.TestSelfServiceRequestConnectionRequest.Types.Details { SourceSystem = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.timekeeping.v1.TimesheetQueryService.</summary>
internal sealed class RouteDocumentFoldersStatusTimesheetQueryTarget(ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient client) : IRouteDocumentFoldersStatusDestination
{
    public async Task DeliverAsync(RouteDocumentFoldersStatusRecord record, CancellationToken token)
    {
        await client.FindTimesheetsAsync(new ApiTimekeeping.FindTimesheetsRequest { OrderBy = record.CorrelationId, Filter = new ApiTimekeeping.FindTimesheetsRequest.Types.Filter { OwnerId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentFolderQueryService/FindDocumentFolders and hands what it keeps to the sink of its kind.</summary>
internal static class RouteDocumentFoldersStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient(channel);
        var got = await source.FindDocumentFoldersAsync(new ApiDocuments.FindDocumentFoldersRequest() { OrderBy = "nightly-route-document-folders-status" }, cancellationToken: token);
        var record = new RouteDocumentFoldersStatusRecord
        {
            Status = got.Result.Status,
            NextPageToken = got.PageInfo.NextPageToken,
            CorrelationId = Text.Normalize(got.Result.CorrelationId),
            Message = got.Result.Message,
        };
        var sinks = new Dictionary<string, IRouteDocumentFoldersStatusDestination>
        {
            ["clock-event-workflow"] = new RouteDocumentFoldersStatusClockEventWorkflowTarget(new ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient(channel)),
            ["self-request-integration"] = new RouteDocumentFoldersStatusSelfServiceRequestIntegrationRelay(new ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient(channel)),
            ["timesheet-query"] = new RouteDocumentFoldersStatusTimesheetQueryTarget(new ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("ROUTE_DOCUMENT_FOLDERS_STATUS_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].DeliverAsync(record, token);
    }
}
