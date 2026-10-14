using Grpc.Net.Client;
using ApiCommon = Acme.Common.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where CarryWarehouseMessage hands each record.</summary>
internal interface ICarryWarehouseMessageWriter
{
    Task WriteAsync(CarryWarehouseMessageRecord record, CancellationToken token);
}

/// <summary>What CarryWarehouseMessage keeps of each PeekWarehouse response.</summary>
internal sealed class CarryWarehouseMessageRecord
{
    public string Message { get; init; } = "";
    public string Code { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.RosterCalculationService.</summary>
internal sealed class CarryWarehouseMessageRosterCalculationRelay(ApiScheduling.RosterCalculationService.RosterCalculationServiceClient client) : ICarryWarehouseMessageWriter
{
    public async Task WriteAsync(CarryWarehouseMessageRecord record, CancellationToken token)
    {
        await client.EstimateRosterAsync(new ApiScheduling.EstimateRosterRequest { RosterId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.identity.v1.UserQueryService.</summary>
internal sealed class CarryWarehouseMessageUserQueryWriter(ApiIdentity.UserQueryService.UserQueryServiceClient client) : ICarryWarehouseMessageWriter
{
    public async Task WriteAsync(CarryWarehouseMessageRecord record, CancellationToken token)
    {
        await client.FindUsersAsync(new ApiIdentity.FindUsersRequest { Filter = new ApiIdentity.FindUsersRequest.Types.Filter { Query = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employeeportal.v1.SelfServiceRequestQueryService.</summary>
internal sealed class CarryWarehouseMessageSelfServiceRequestQueryRelay : ICarryWarehouseMessageWriter
{
    private readonly ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient _client;

    public CarryWarehouseMessageSelfServiceRequestQueryRelay(ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient client) => _client = client;

    public async Task WriteAsync(CarryWarehouseMessageRecord record, CancellationToken token)
    {
        await _client.SuggestSelfServiceRequestsAsync(new ApiEmployeeportal.SuggestSelfServiceRequestsRequest { Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId }, Filter = new ApiEmployeeportal.SuggestSelfServiceRequestsRequest.Types.Filter { OwnerId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingTaskWorkflowService.</summary>
internal sealed class CarryWarehouseMessageOnboardingTaskWorkflowSink(ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient client) : ICarryWarehouseMessageWriter
{
    public async Task WriteAsync(CarryWarehouseMessageRecord record, CancellationToken token)
    {
        await client.AdvanceOnboardingTaskWorkflowAsync(new ApiOnboarding.AdvanceOnboardingTaskWorkflowRequest { Reason = record.Status, Details = new ApiOnboarding.AdvanceOnboardingTaskWorkflowRequest.Types.Details { InitiatedBy = record.Code } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CarryWarehouseMessageLogOnly : ICarryWarehouseMessageWriter
{
    public Task WriteAsync(CarryWarehouseMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.warehousing.v1.WarehouseQueryService/PeekWarehouse and hands what it keeps to the sink of its kind.</summary>
internal static class CarryWarehouseMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient(channel);
        var response = await source.PeekWarehouseAsync(new ApiWarehousing.PeekWarehouseRequest() { WarehouseId = "main-carry-warehouse-message" }, cancellationToken: token);
        var record = new CarryWarehouseMessageRecord
        {
            Message = response.Result.Message,
            Code = response.Warehouse.Code,
            Status = response.Result.Status,
            CorrelationId = response.Result.CorrelationId,
        };
        await DispatchCarryWarehouseMessageAsync("onboarding-task-workflow", record, channel, token);
    }

    private static async Task DispatchCarryWarehouseMessageAsync(string kind, CarryWarehouseMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<CarryWarehouseMessageRecord, CancellationToken, Task> write = new CarryWarehouseMessageLogOnly().WriteAsync;
        switch (kind)
        {
            case "roster-calculation":
                write = new CarryWarehouseMessageRosterCalculationRelay(new ApiScheduling.RosterCalculationService.RosterCalculationServiceClient(channel)).WriteAsync;
                break;
            case "user-query":
                write = new CarryWarehouseMessageUserQueryWriter(new ApiIdentity.UserQueryService.UserQueryServiceClient(channel)).WriteAsync;
                break;
            case "self-request-query":
                write = new CarryWarehouseMessageSelfServiceRequestQueryRelay(new ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient(channel)).WriteAsync;
                break;
            case "onboarding-task-workflow":
                write = new CarryWarehouseMessageOnboardingTaskWorkflowSink(new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel)).WriteAsync;
                break;
        }
        await write(record, token);
    }
}
