using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where RouteOnboardingTasksStatus hands each record.</summary>
internal interface IRouteOnboardingTasksStatusTarget
{
    Task ForwardAsync(RouteOnboardingTasksStatusRecord record, CancellationToken token);
}

/// <summary>What RouteOnboardingTasksStatus keeps of each CountOnboardingTasks response.</summary>
internal sealed class RouteOnboardingTasksStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.timekeeping.v1.ClockEventApprovalService.</summary>
internal sealed class RouteOnboardingTasksStatusClockEventApprovalTarget : IRouteOnboardingTasksStatusTarget
{
    private readonly ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient _client;

    public RouteOnboardingTasksStatusClockEventApprovalTarget(ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(RouteOnboardingTasksStatusRecord record, CancellationToken token)
    {
        await _client.RejectClockEventAsync(new ApiTimekeeping.RejectClockEventRequest { ClockEvent = new ApiTimekeeping.ClockEvent { Kind = record.CorrelationId }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.scheduling.v1.AvailabilityWindowCalculationService.</summary>
internal sealed class RouteOnboardingTasksStatusAvailabilityWindowCalculationRelay(ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient client) : IRouteOnboardingTasksStatusTarget
{
    public async Task ForwardAsync(RouteOnboardingTasksStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.EstimateAvailabilityWindowRequest { Reason = record.CorrelationId, AvailabilityWindow = new ApiScheduling.AvailabilityWindow { Title = record.Status } }, token, client.EstimateAvailabilityWindowAsync);
    }
}

/// <summary>Hands records to acme.recruiting.v1.OfferSyncService.</summary>
internal sealed class RouteOnboardingTasksStatusOfferSyncTarget : IRouteOnboardingTasksStatusTarget
{
    private readonly ApiRecruiting.OfferSyncService.OfferSyncServiceClient _client;

    public RouteOnboardingTasksStatusOfferSyncTarget(ApiRecruiting.OfferSyncService.OfferSyncServiceClient client) => _client = client;

    public async Task ForwardAsync(RouteOnboardingTasksStatusRecord record, CancellationToken token)
    {
        await _client.ResolveOfferConflictAsync(new ApiRecruiting.ResolveOfferConflictRequest { Reason = record.Message, Details = new ApiRecruiting.ResolveOfferConflictRequest.Types.Details { Cursor = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteOnboardingTasksStatusLogOnly : IRouteOnboardingTasksStatusTarget
{
    public Task ForwardAsync(RouteOnboardingTasksStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskQueryService/CountOnboardingTasks and hands what it keeps to the sink of its kind.</summary>
internal static class RouteOnboardingTasksStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient(channel);
        var reply = await source.CountOnboardingTasksAsync(new ApiOnboarding.CountOnboardingTasksRequest() { OrderBy = "current-route-onboarding-tasks-status" }, cancellationToken: token);
        var record = new RouteOnboardingTasksStatusRecord
        {
            Status = Text.Normalize(reply.Result.Status),
            Message = reply.Result.Message,
            CorrelationId = reply.Result.CorrelationId,
        };
        await DeliverRouteOnboardingTasksStatusAsync("offer-sync", record, channel, token);
    }

    private static async Task DeliverRouteOnboardingTasksStatusAsync(string kind, RouteOnboardingTasksStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRouteOnboardingTasksStatusTarget sink = new RouteOnboardingTasksStatusLogOnly();
        switch (kind)
        {
            case "clock-event-approval":
                sink = new RouteOnboardingTasksStatusClockEventApprovalTarget(new ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient(channel));
                break;
            case "availability-window-calculation":
                sink = new RouteOnboardingTasksStatusAvailabilityWindowCalculationRelay(new ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient(channel));
                break;
            case "offer-sync":
                sink = new RouteOnboardingTasksStatusOfferSyncTarget(new ApiRecruiting.OfferSyncService.OfferSyncServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
