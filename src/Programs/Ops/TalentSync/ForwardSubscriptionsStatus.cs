using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAssets = Acme.Assets.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiSubscriptions = Acme.Subscriptions.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where ForwardSubscriptionsStatus hands each record.</summary>
internal interface IForwardSubscriptionsStatusTarget
{
    Task ApplyAsync(ForwardSubscriptionsStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardSubscriptionsStatus keeps of each SearchSubscriptions response.</summary>
internal sealed class ForwardSubscriptionsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.assets.v1.FixedAssetCalculationService.</summary>
internal sealed class ForwardSubscriptionsStatusFixedAssetCalculationRelay(ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient client) : IForwardSubscriptionsStatusTarget
{
    public async Task ApplyAsync(ForwardSubscriptionsStatusRecord record, CancellationToken token)
    {
        await client.RecalculateFixedAssetsAsync(new ApiAssets.RecalculateFixedAssetsRequest { FixedAsset = new ApiAssets.FixedAsset { Code = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employees.v1.OrgUnitApprovalService.</summary>
internal sealed class ForwardSubscriptionsStatusOrgUnitApprovalTarget : IForwardSubscriptionsStatusTarget
{
    private readonly ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient _client;

    public ForwardSubscriptionsStatusOrgUnitApprovalTarget(ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(ForwardSubscriptionsStatusRecord record, CancellationToken token)
    {
        await _client.SubmitOrgUnitAsync(new ApiEmployees.SubmitOrgUnitRequest { OrgUnit = new ApiEmployees.OrgUnit { OrgUnitId = record.CorrelationId, Name = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassValidationService.</summary>
internal sealed class ForwardSubscriptionsStatusVisitorPassValidationRelay(ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient client) : IForwardSubscriptionsStatusTarget
{
    public async Task ApplyAsync(ForwardSubscriptionsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.ValidateVisitorPassRequest { Reason = record.CorrelationId }, token, client.ValidateVisitorPassAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardSubscriptionsStatusLogOnly : IForwardSubscriptionsStatusTarget
{
    public Task ApplyAsync(ForwardSubscriptionsStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.subscriptions.v1.SubscriptionService/SearchSubscriptions and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardSubscriptionsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiSubscriptions.SubscriptionService.SubscriptionServiceClient(channel);
        var reply = await source.SearchSubscriptionsAsync(new ApiSubscriptions.SearchSubscriptionsRequest() { OrderBy = "nightly-forward-subscriptions-status" }, cancellationToken: token);
        var record = new ForwardSubscriptionsStatusRecord
        {
            Status = reply.Result.Status,
            Message = reply.Result.Message,
            CorrelationId = reply.Result.CorrelationId,
        };
        await DispatchForwardSubscriptionsStatusAsync("org-unit-approval", record, channel, token);
    }

    private static async Task DispatchForwardSubscriptionsStatusAsync(string kind, ForwardSubscriptionsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<ForwardSubscriptionsStatusRecord, CancellationToken, Task> write = new ForwardSubscriptionsStatusLogOnly().ApplyAsync;
        switch (kind)
        {
            case "fixed-asset-calculation":
                write = new ForwardSubscriptionsStatusFixedAssetCalculationRelay(new ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient(channel)).ApplyAsync;
                break;
            case "org-unit-approval":
                write = new ForwardSubscriptionsStatusOrgUnitApprovalTarget(new ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient(channel)).ApplyAsync;
                break;
            case "visitor-pass-validation":
                write = new ForwardSubscriptionsStatusVisitorPassValidationRelay(new ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient(channel)).ApplyAsync;
                break;
        }
        await write(record, token);
    }
}
