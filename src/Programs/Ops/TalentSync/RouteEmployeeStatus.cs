using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCheckout = Acme.Checkout.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFuel = Acme.Fuel.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where RouteEmployeeStatus hands each record.</summary>
internal interface IRouteEmployeeStatusSink
{
    Task WriteAsync(RouteEmployeeStatusRecord record, CancellationToken token);
}

/// <summary>What RouteEmployeeStatus keeps of each EscalateEmployee response.</summary>
internal sealed class RouteEmployeeStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class RouteEmployeeStatusCheckoutSessionWriter(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) : IRouteEmployeeStatusSink
{
    public async Task WriteAsync(RouteEmployeeStatusRecord record, CancellationToken token)
    {
        await client.GetCheckoutSessionAsync(new ApiCheckout.GetCheckoutSessionRequest { CheckoutSessionId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.fuel.v1.FuelCardQueryService.</summary>
internal sealed class RouteEmployeeStatusFuelCardQuerySink(ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient client) : IRouteEmployeeStatusSink
{
    public async Task WriteAsync(RouteEmployeeStatusRecord record, CancellationToken token)
    {
        await client.CountFuelCardsAsync(new ApiFuel.CountFuelCardsRequest { Filter = new ApiFuel.CountFuelCardsRequest.Types.Filter { Region = record.Message, OwnerId = record.CorrelationId }, OrderBy = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employees.v1.EmployeeApprovalService/EscalateEmployee and hands what it keeps to the sink of its kind.</summary>
internal static class RouteEmployeeStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient(channel);
        var got = await source.EscalateEmployeeAsync(new ApiEmployees.EscalateEmployeeRequest() { Reason = "live-route-employee-status" }, cancellationToken: token);
        var record = new RouteEmployeeStatusRecord
        {
            Status = Text.Normalize(got.Result.Status),
            ReferenceId = Text.Normalize(got.Outcome.ReferenceId),
            Message = got.Outcome.Message,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IRouteEmployeeStatusSink>
        {
            ["checkout-session"] = new RouteEmployeeStatusCheckoutSessionWriter(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel)),
            ["fuel-card-query"] = new RouteEmployeeStatusFuelCardQuerySink(new ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient(channel)),
        };
        await RouteRouteEmployeeStatusAsync("checkout-session", record, sinks, token);
    }

    private static async Task RouteRouteEmployeeStatusAsync(string kind, RouteEmployeeStatusRecord record, IReadOnlyDictionary<string, IRouteEmployeeStatusSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
