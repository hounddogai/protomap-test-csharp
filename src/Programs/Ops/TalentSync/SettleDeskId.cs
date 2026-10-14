using Grpc.Net.Client;
using ApiDevices = Acme.Devices.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where SettleDeskId hands each record.</summary>
internal interface ISettleDeskIdDestination
{
    Task PushAsync(SettleDeskIdRecord record, CancellationToken token);
}

/// <summary>What SettleDeskId keeps of each PreviewDesk response.</summary>
internal sealed class SettleDeskIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingPlanCalculationService.</summary>
internal sealed class SettleDeskIdOnboardingPlanCalculationRelay(ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient client) : ISettleDeskIdDestination
{
    public async Task PushAsync(SettleDeskIdRecord record, CancellationToken token)
    {
        await client.EstimateOnboardingPlanAsync(new ApiOnboarding.EstimateOnboardingPlanRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.InterviewValidationService.</summary>
internal sealed class SettleDeskIdInterviewValidationTarget : ISettleDeskIdDestination
{
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _client;

    public SettleDeskIdInterviewValidationTarget(ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient client) => _client = client;

    public async Task PushAsync(SettleDeskIdRecord record, CancellationToken token)
    {
        await _client.GetInterviewRulesAsync(new ApiRecruiting.GetInterviewRulesRequest { InterviewId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.DevicePolicyWorkflowService.</summary>
internal sealed class SettleDeskIdDevicePolicyWorkflowTarget(ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient client) : ISettleDeskIdDestination
{
    public async Task PushAsync(SettleDeskIdRecord record, CancellationToken token)
    {
        await client.InspectDevicePolicyWorkflowAsync(new ApiDevices.InspectDevicePolicyWorkflowRequest { DevicePolicyId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.forecasting.v1.DemandForecastQueryService.</summary>
internal sealed class SettleDeskIdDemandForecastQueryWriter : ISettleDeskIdDestination
{
    private readonly ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient _client;

    public SettleDeskIdDemandForecastQueryWriter(ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient client) => _client = client;

    public async Task PushAsync(SettleDeskIdRecord record, CancellationToken token)
    {
        await _client.FindDemandForecastsAsync(new ApiForecasting.FindDemandForecastsRequest { Filter = new ApiForecasting.FindDemandForecastsRequest.Types.Filter { Region = record.Message, OwnerId = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.DeskValidationService/PreviewDesk and hands what it keeps to the sink of its kind.</summary>
internal static class SettleDeskId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.DeskValidationService.DeskValidationServiceClient(channel);
        var reply = await source.PreviewDeskAsync(new ApiWorkplaces.PreviewDeskRequest() { DeskId = "current-settle-desk-id" }, cancellationToken: token);
        var record = new SettleDeskIdRecord
        {
            ReferenceId = reply.Outcome.ReferenceId,
            Message = reply.Outcome.Message,
        };
        await ActOnSettleDeskIdAsync("interview-validation", record, channel, token);
    }

    private static async Task ActOnSettleDeskIdAsync(string kind, SettleDeskIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["onboarding-plan-calculation"] = () => new SettleDeskIdOnboardingPlanCalculationRelay(new ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient(channel)).PushAsync(record, token),
            ["interview-validation"] = () => new SettleDeskIdInterviewValidationTarget(new ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient(channel)).PushAsync(record, token),
            ["device-policy-workflow"] = () => new SettleDeskIdDevicePolicyWorkflowTarget(new ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient(channel)).PushAsync(record, token),
            ["demand-forecast-query"] = () => new SettleDeskIdDemandForecastQueryWriter(new ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
