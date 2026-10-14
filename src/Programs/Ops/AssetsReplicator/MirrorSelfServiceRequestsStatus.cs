using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBudgeting = Acme.Budgeting.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where MirrorSelfServiceRequestsStatus hands each record.</summary>
internal interface IMirrorSelfServiceRequestsStatusSink
{
    Task ApplyAsync(MirrorSelfServiceRequestsStatusRecord record, CancellationToken token);
}

/// <summary>What MirrorSelfServiceRequestsStatus keeps of each BrowseSelfServiceRequests response.</summary>
internal sealed class MirrorSelfServiceRequestsStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Writes records to acme.travel.v1.TripCalculationService.</summary>
internal sealed class MirrorSelfServiceRequestsStatusTripCalculationRelay(ApiTravel.TripCalculationService.TripCalculationServiceClient client) : IMirrorSelfServiceRequestsStatusSink
{
    public async Task ApplyAsync(MirrorSelfServiceRequestsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.CalculateTripRequest { Trip = new ApiTravel.Trip { TripId = record.Status } }, token, client.CalculateTripAsync);
    }
}

/// <summary>Passes records to acme.budgeting.v1.BudgetCalculationService.</summary>
internal sealed class MirrorSelfServiceRequestsStatusBudgetCalculationWriter(ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient client) : IMirrorSelfServiceRequestsStatusSink
{
    public async Task ApplyAsync(MirrorSelfServiceRequestsStatusRecord record, CancellationToken token)
    {
        await client.EstimateBudgetAsync(new ApiBudgeting.EstimateBudgetRequest { BudgetId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employeeportal.v1.SelfServiceRequestQueryService/BrowseSelfServiceRequests and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorSelfServiceRequestsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient(channel);
        var response = await source.BrowseSelfServiceRequestsAsync(new ApiEmployeeportal.BrowseSelfServiceRequestsRequest() { OrderBy = "default-mirror-self-service-requests-status" }, cancellationToken: token);
        var record = new MirrorSelfServiceRequestsStatusRecord
        {
            Status = response.Result.Status,
            NextPageToken = Text.Normalize(response.PageInfo.NextPageToken),
        };
        var sinks = new IMirrorSelfServiceRequestsStatusSink[]
        {
            new MirrorSelfServiceRequestsStatusTripCalculationRelay(new ApiTravel.TripCalculationService.TripCalculationServiceClient(channel)),
            new MirrorSelfServiceRequestsStatusBudgetCalculationWriter(new ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient(channel)),
        };
        await sinks[0].ApplyAsync(record, token);
    }
}
