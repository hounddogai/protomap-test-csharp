using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiRouting = Acme.Routing.V1;
using ApiWebbff = Acme.Webbff.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where CarryIncidentOptionsId hands each record.</summary>
internal interface ICarryIncidentOptionsIdWriter
{
    Task ForwardAsync(CarryIncidentOptionsIdRecord record, CancellationToken token);
}

/// <summary>What CarryIncidentOptionsId keeps of each CompareIncidentOptions response.</summary>
internal sealed class CarryIncidentOptionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Hands records to acme.ledger.v1.LedgerAccountService.</summary>
internal sealed class CarryIncidentOptionsIdLedgerAccountTarget(ApiLedger.LedgerAccountService.LedgerAccountServiceClient client) : ICarryIncidentOptionsIdWriter
{
    public async Task ForwardAsync(CarryIncidentOptionsIdRecord record, CancellationToken token)
    {
        await client.ListLedgerAccountsAsync(new ApiLedger.ListLedgerAccountsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiLedger.ListLedgerAccountsRequest.Types.Filter { OwnerId = record.CorrelationId, Query = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.routing.v1.RouteCalculationService.</summary>
internal sealed class CarryIncidentOptionsIdRouteCalculationWriter : ICarryIncidentOptionsIdWriter
{
    private readonly ApiRouting.RouteCalculationService.RouteCalculationServiceClient _client;

    public CarryIncidentOptionsIdRouteCalculationWriter(ApiRouting.RouteCalculationService.RouteCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryIncidentOptionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRouting.SimulateRouteRequest { Route = new ApiRouting.Route { Notes = record.CurrencyCode } }, token, _client.SimulateRouteAsync);
    }
}

/// <summary>Writes records to acme.learning.v1.CourseWorkflowService.</summary>
internal sealed class CarryIncidentOptionsIdCourseWorkflowRelay : ICarryIncidentOptionsIdWriter
{
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _client;

    public CarryIncidentOptionsIdCourseWorkflowRelay(ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryIncidentOptionsIdRecord record, CancellationToken token)
    {
        await _client.StartCourseWorkflowAsync(new ApiLearning.StartCourseWorkflowRequest { Course = new ApiLearning.Course { Name = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.webbff.v1.PageCompositionQueryService.</summary>
internal sealed class CarryIncidentOptionsIdPageCompositionQueryWriter : ICarryIncidentOptionsIdWriter
{
    private readonly ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient _client;

    public CarryIncidentOptionsIdPageCompositionQueryWriter(ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryIncidentOptionsIdRecord record, CancellationToken token)
    {
        await _client.BrowsePageCompositionsAsync(new ApiWebbff.BrowsePageCompositionsRequest { Filter = new ApiWebbff.BrowsePageCompositionsRequest.Types.Filter { Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.IncidentCalculationService/CompareIncidentOptions and hands what it keeps to the sink of its kind.</summary>
internal static class CarryIncidentOptionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient(channel);
        var response = await source.CompareIncidentOptionsAsync(new ApiItsm.CompareIncidentOptionsRequest() { IncidentId = "current-carry-incident-options-id" }, cancellationToken: token);
        var record = new CarryIncidentOptionsIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Status = response.Result.Status,
            Message = Text.Normalize(response.Result.Message),
            CurrencyCode = response.Breakdown.CurrencyCode,
        };
        await ActOnCarryIncidentOptionsIdAsync("course-workflow", record, channel, token);
    }

    private static async Task ActOnCarryIncidentOptionsIdAsync(string kind, CarryIncidentOptionsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["ledger-account"] = () => new CarryIncidentOptionsIdLedgerAccountTarget(new ApiLedger.LedgerAccountService.LedgerAccountServiceClient(channel)).ForwardAsync(record, token),
            ["route-calculation"] = () => new CarryIncidentOptionsIdRouteCalculationWriter(new ApiRouting.RouteCalculationService.RouteCalculationServiceClient(channel)).ForwardAsync(record, token),
            ["course-workflow"] = () => new CarryIncidentOptionsIdCourseWorkflowRelay(new ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient(channel)).ForwardAsync(record, token),
            ["page-composition-query"] = () => new CarryIncidentOptionsIdPageCompositionQueryWriter(new ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
