using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiEmployees = Acme.Employees.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where PropagateEmployeeId hands each record.</summary>
internal interface IPropagateEmployeeIdDestination
{
    Task ApplyAsync(PropagateEmployeeIdRecord record, CancellationToken token);
}

/// <summary>What PropagateEmployeeId keeps of each RecallEmployee response.</summary>
internal sealed class PropagateEmployeeIdRecord
{
    public string EmployeeId { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.licensing.v1.EntitlementCalculationService.</summary>
internal sealed class PropagateEmployeeIdEntitlementCalculationRelay(ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient client) : IPropagateEmployeeIdDestination
{
    public async Task ApplyAsync(PropagateEmployeeIdRecord record, CancellationToken token)
    {
        await client.CompareEntitlementOptionsAsync(new ApiLicensing.CompareEntitlementOptionsRequest { Details = new ApiLicensing.CompareEntitlementOptionsRequest.Types.Details { CurrencyCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelBookingLifecycleService.</summary>
internal sealed class PropagateEmployeeIdTravelBookingLifecycleTarget : IPropagateEmployeeIdDestination
{
    private readonly ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient _client;

    public PropagateEmployeeIdTravelBookingLifecycleTarget(ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateEmployeeIdRecord record, CancellationToken token)
    {
        await _client.ExpireTravelBookingAsync(new ApiTravel.ExpireTravelBookingRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateEmployeeIdLogOnly : IPropagateEmployeeIdDestination
{
    public Task ApplyAsync(PropagateEmployeeIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.EmployeeId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.employees.v1.EmployeeApprovalService/RecallEmployee and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateEmployeeId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient(channel);
        var reply = await source.RecallEmployeeAsync(new ApiEmployees.RecallEmployeeRequest() { Reason = "nightly-propagate-employee-id" }, cancellationToken: token);
        var record = new PropagateEmployeeIdRecord
        {
            EmployeeId = Text.Normalize(reply.Employee.EmployeeId),
            Status = reply.Result.Status,
            ReferenceId = reply.Outcome.ReferenceId,
            Message = reply.Result.Message,
        };
        await DeliverPropagateEmployeeIdAsync("travel-booking-lifecycle", record, channel, token);
    }

    private static async Task DeliverPropagateEmployeeIdAsync(string kind, PropagateEmployeeIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagateEmployeeIdDestination sink = kind switch
        {
            "entitlement-calculation" => new PropagateEmployeeIdEntitlementCalculationRelay(new ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient(channel)),
            "travel-booking-lifecycle" => new PropagateEmployeeIdTravelBookingLifecycleTarget(new ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient(channel)),
            _ => new PropagateEmployeeIdLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
