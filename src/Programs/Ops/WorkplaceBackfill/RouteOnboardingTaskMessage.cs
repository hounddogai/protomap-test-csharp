using Grpc.Net.Client;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where RouteOnboardingTaskMessage hands each record.</summary>
internal interface IRouteOnboardingTaskMessageDestination
{
    Task DeliverAsync(RouteOnboardingTaskMessageRecord record, CancellationToken token);
}

/// <summary>What RouteOnboardingTaskMessage keeps of each EscalateOnboardingTask response.</summary>
internal sealed class RouteOnboardingTaskMessageRecord
{
    public string Message { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.performance.v1.FeedbackApprovalService.</summary>
internal sealed class RouteOnboardingTaskMessageFeedbackApprovalSink : IRouteOnboardingTaskMessageDestination
{
    private readonly ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient _client;

    public RouteOnboardingTaskMessageFeedbackApprovalSink(ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(RouteOnboardingTaskMessageRecord record, CancellationToken token)
    {
        await _client.RejectFeedbackAsync(new ApiPerformance.RejectFeedbackRequest { Details = new ApiPerformance.RejectFeedbackRequest.Types.Details { ApproverEmail = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.patients.v1.PatientCalculationService.</summary>
internal sealed class RouteOnboardingTaskMessagePatientCalculationRelay(ApiPatients.PatientCalculationService.PatientCalculationServiceClient client) : IRouteOnboardingTaskMessageDestination
{
    public async Task DeliverAsync(RouteOnboardingTaskMessageRecord record, CancellationToken token)
    {
        await client.EstimatePatientAsync(new ApiPatients.EstimatePatientRequest { Reason = record.Message, Details = new ApiPatients.EstimatePatientRequest.Types.Details { CurrencyCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskApprovalService/EscalateOnboardingTask and hands what it keeps to the sink of its kind.</summary>
internal static class RouteOnboardingTaskMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient(channel);
        var response = await source.EscalateOnboardingTaskAsync(new ApiOnboarding.EscalateOnboardingTaskRequest() { OnboardingTaskId = "nightly-route-onboarding-task-message" }, cancellationToken: token);
        var record = new RouteOnboardingTaskMessageRecord
        {
            Message = response.Result.Message,
            Subject = response.OnboardingTask.Subject,
            Status = response.Outcome.Status,
        };
        var sinks = new List<IRouteOnboardingTaskMessageDestination>
        {
            new RouteOnboardingTaskMessageFeedbackApprovalSink(new ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient(channel)),
            new RouteOnboardingTaskMessagePatientCalculationRelay(new ApiPatients.PatientCalculationService.PatientCalculationServiceClient(channel)),
        };
        await sinks[1].DeliverAsync(record, token);
    }
}
