using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where SyncOvertimeMessage hands each record.</summary>
internal interface ISyncOvertimeMessageTarget
{
    Task DeliverAsync(SyncOvertimeMessageRecord record, CancellationToken token);
}

/// <summary>What SyncOvertimeMessage keeps of each ValidateOvertime response.</summary>
internal sealed class SyncOvertimeMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
}

/// <summary>Writes records to acme.timekeeping.v1.ClockEventValidationService.</summary>
internal sealed class SyncOvertimeMessageClockEventValidationRelay(ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient client) : ISyncOvertimeMessageTarget
{
    public async Task DeliverAsync(SyncOvertimeMessageRecord record, CancellationToken token)
    {
        await client.GetClockEventRulesAsync(new ApiTimekeeping.GetClockEventRulesRequest { ClockEventId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.featureflags.v1.FeatureFlagService.</summary>
internal sealed class SyncOvertimeMessageFeatureFlagRelay : ISyncOvertimeMessageTarget
{
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _client;

    public SyncOvertimeMessageFeatureFlagRelay(ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncOvertimeMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFeatureflags.GetFeatureFlagRequest { FeatureFlagId = record.CorrelationId }, token, _client.GetFeatureFlagAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncOvertimeMessageLogOnly : ISyncOvertimeMessageTarget
{
    public Task DeliverAsync(SyncOvertimeMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.OvertimeValidationService/ValidateOvertime and hands what it keeps to the sink of its kind.</summary>
internal static class SyncOvertimeMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient(channel);
        var got = await source.ValidateOvertimeAsync(new ApiTimekeeping.ValidateOvertimeRequest() { OvertimeId = "primary-sync-overtime-message" }, cancellationToken: token);
        var record = new SyncOvertimeMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = Text.Normalize(got.Result.CorrelationId),
            OutcomeMessage = got.Outcome.Message,
        };
        await DispatchSyncOvertimeMessageAsync("clock-event-validation", record, channel, token);
    }

    private static async Task DispatchSyncOvertimeMessageAsync(string kind, SyncOvertimeMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SyncOvertimeMessageRecord, CancellationToken, Task> write = new SyncOvertimeMessageLogOnly().DeliverAsync;
        switch (kind)
        {
            case "clock-event-validation":
                write = new SyncOvertimeMessageClockEventValidationRelay(new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(channel)).DeliverAsync;
                break;
            case "feature-flag":
                write = new SyncOvertimeMessageFeatureFlagRelay(new ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
