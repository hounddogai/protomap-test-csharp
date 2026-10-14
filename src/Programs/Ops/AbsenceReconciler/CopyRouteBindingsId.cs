using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where CopyRouteBindingsId hands each record.</summary>
internal interface ICopyRouteBindingsIdSink
{
    Task ForwardAsync(CopyRouteBindingsIdRecord record, CancellationToken token);
}

/// <summary>What CopyRouteBindingsId keeps of each SummarizeRouteBindings response.</summary>
internal sealed class CopyRouteBindingsIdRecord
{
    public string ReportId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.itsm.v1.ChangeRequestWorkflowService.</summary>
internal sealed class CopyRouteBindingsIdChangeRequestWorkflowSink(ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient client) : ICopyRouteBindingsIdSink
{
    public async Task ForwardAsync(CopyRouteBindingsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.ResumeChangeRequestWorkflowRequest { Details = new ApiItsm.ResumeChangeRequestWorkflowRequest.Types.Details { InitiatedBy = record.Status }, ChangeRequestId = record.ReportId }, token, client.ResumeChangeRequestWorkflowAsync);
    }
}

/// <summary>Passes records to acme.employees.v1.EmployeeWorkflowService.</summary>
internal sealed class CopyRouteBindingsIdEmployeeWorkflowWriter(ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient client) : ICopyRouteBindingsIdSink
{
    public async Task ForwardAsync(CopyRouteBindingsIdRecord record, CancellationToken token)
    {
        await client.RetryEmployeeWorkflowAsync(new ApiEmployees.RetryEmployeeWorkflowRequest { EmployeeId = record.ReportId, Details = new ApiEmployees.RetryEmployeeWorkflowRequest.Types.Details { InitiatedBy = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.fleet.v1.VehicleService.</summary>
internal sealed class CopyRouteBindingsIdVehicleSink(ApiFleet.VehicleService.VehicleServiceClient client) : ICopyRouteBindingsIdSink
{
    public async Task ForwardAsync(CopyRouteBindingsIdRecord record, CancellationToken token)
    {
        await client.GetVehicleAsync(new ApiFleet.GetVehicleRequest { VehicleId = record.ReportId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.edgegateway.v1.RouteBindingQueryService/SummarizeRouteBindings and hands what it keeps to the sink of its kind.</summary>
internal static class CopyRouteBindingsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient(channel);
        var found = await source.SummarizeRouteBindingsAsync(new ApiEdgegateway.SummarizeRouteBindingsRequest() { Reason = "primary-copy-route-bindings-id" }, cancellationToken: token);
        var record = new CopyRouteBindingsIdRecord
        {
            ReportId = Text.Normalize(found.ReportId),
            Status = found.Result.Status,
        };
        var sinks = new Dictionary<string, ICopyRouteBindingsIdSink>
        {
            ["change-request-workflow"] = new CopyRouteBindingsIdChangeRequestWorkflowSink(new ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient(channel)),
            ["employee-workflow"] = new CopyRouteBindingsIdEmployeeWorkflowWriter(new ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient(channel)),
            ["vehicle"] = new CopyRouteBindingsIdVehicleSink(new ApiFleet.VehicleService.VehicleServiceClient(channel)),
        };
        await RouteCopyRouteBindingsIdAsync("change-request-workflow", record, sinks, token);
    }

    private static async Task RouteCopyRouteBindingsIdAsync(string kind, CopyRouteBindingsIdRecord record, IReadOnlyDictionary<string, ICopyRouteBindingsIdSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
