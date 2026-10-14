using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiConcessions = Acme.Concessions.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where HandOffWebhookEndpointMessage hands each record.</summary>
internal interface IHandOffWebhookEndpointMessageSink
{
    Task ApplyAsync(HandOffWebhookEndpointMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffWebhookEndpointMessage keeps of each LookupWebhookEndpoint response.</summary>
internal sealed class HandOffWebhookEndpointMessageRecord
{
    public string Message { get; init; } = "";
    public string Code { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.featureflags.v1.FeatureFlagQueryService.</summary>
internal sealed class HandOffWebhookEndpointMessageFeatureFlagQueryWriter(ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient client) : IHandOffWebhookEndpointMessageSink
{
    public async Task ApplyAsync(HandOffWebhookEndpointMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFeatureflags.PeekFeatureFlagRequest { FeatureFlagId = record.Code }, token, client.PeekFeatureFlagAsync);
    }
}

/// <summary>Hands records to acme.concessions.v1.ConcessionStandCalculationService.</summary>
internal sealed class HandOffWebhookEndpointMessageConcessionStandCalculationTarget(ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient client) : IHandOffWebhookEndpointMessageSink
{
    public async Task ApplyAsync(HandOffWebhookEndpointMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiConcessions.QuoteConcessionStandRequest { Details = new ApiConcessions.QuoteConcessionStandRequest.Types.Details { CurrencyCode = record.Message }, ConcessionStandId = record.Code, Reason = record.CorrelationId }, token, client.QuoteConcessionStandAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffWebhookEndpointMessageLogOnly : IHandOffWebhookEndpointMessageSink
{
    public Task ApplyAsync(HandOffWebhookEndpointMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.webhooks.v1.WebhookEndpointQueryService/LookupWebhookEndpoint and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffWebhookEndpointMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient(channel);
        var got = await source.LookupWebhookEndpointAsync(new ApiWebhooks.LookupWebhookEndpointRequest() { WebhookEndpointId = "current-hand-off-webhook-endpoint-message" }, cancellationToken: token);
        var record = new HandOffWebhookEndpointMessageRecord
        {
            Message = Text.Normalize(got.Result.Message),
            Code = got.WebhookEndpoint.Code,
            CorrelationId = got.Result.CorrelationId,
        };
        await DispatchHandOffWebhookEndpointMessageAsync("feature-flag-query", record, channel, token);
    }

    private static async Task DispatchHandOffWebhookEndpointMessageAsync(string kind, HandOffWebhookEndpointMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<HandOffWebhookEndpointMessageRecord, CancellationToken, Task> write = new HandOffWebhookEndpointMessageLogOnly().ApplyAsync;
        switch (kind)
        {
            case "feature-flag-query":
                write = new HandOffWebhookEndpointMessageFeatureFlagQueryWriter(new ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient(channel)).ApplyAsync;
                break;
            case "concession-stand-calculation":
                write = new HandOffWebhookEndpointMessageConcessionStandCalculationTarget(new ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient(channel)).ApplyAsync;
                break;
        }
        await write(record, token);
    }
}
