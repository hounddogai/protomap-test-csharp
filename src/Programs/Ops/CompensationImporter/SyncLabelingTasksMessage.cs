using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where SyncLabelingTasksMessage hands each record.</summary>
internal interface ISyncLabelingTasksMessageTarget
{
    Task ApplyAsync(SyncLabelingTasksMessageRecord record, CancellationToken token);
}

/// <summary>What SyncLabelingTasksMessage keeps of each ResolveLabelingTasks response.</summary>
internal sealed class SyncLabelingTasksMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.driverapp.v1.DriverHomeService.</summary>
internal sealed class SyncLabelingTasksMessageDriverHomeTarget(ApiDriverapp.DriverHomeService.DriverHomeServiceClient client) : ISyncLabelingTasksMessageTarget
{
    public async Task ApplyAsync(SyncLabelingTasksMessageRecord record, CancellationToken token)
    {
        await client.GetDriverHomeAsync(new ApiDriverapp.GetDriverHomeRequest { DriverHomeId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingTaskCalculationService.</summary>
internal sealed class SyncLabelingTasksMessageOnboardingTaskCalculationWriter : ISyncLabelingTasksMessageTarget
{
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _client;

    public SyncLabelingTasksMessageOnboardingTaskCalculationWriter(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncLabelingTasksMessageRecord record, CancellationToken token)
    {
        await _client.GetOnboardingTaskRatesAsync(new ApiOnboarding.GetOnboardingTaskRatesRequest { OnboardingTaskId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.labeling.v1.LabelingTaskQueryService/ResolveLabelingTasks and hands what it keeps to the sink of its kind.</summary>
internal static class SyncLabelingTasksMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient(channel);
        var response = await source.ResolveLabelingTasksAsync(new ApiLabeling.ResolveLabelingTasksRequest(), cancellationToken: token);
        var record = new SyncLabelingTasksMessageRecord
        {
            Message = response.Result.Message,
            Status = response.Result.Status,
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
        };
        await ActOnSyncLabelingTasksMessageAsync("onboarding-task-calculation", record, channel, token);
    }

    private static async Task ActOnSyncLabelingTasksMessageAsync(string kind, SyncLabelingTasksMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["driver-home"] = () => new SyncLabelingTasksMessageDriverHomeTarget(new ApiDriverapp.DriverHomeService.DriverHomeServiceClient(channel)).ApplyAsync(record, token),
            ["onboarding-task-calculation"] = () => new SyncLabelingTasksMessageOnboardingTaskCalculationWriter(new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
