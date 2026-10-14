using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiCart = Acme.Cart.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where SyncCartsId hands each record.</summary>
internal interface ISyncCartsIdDestination
{
    Task DeliverAsync(SyncCartsIdRecord record, CancellationToken token);
}

/// <summary>What SyncCartsId keeps of each ListCarts response.</summary>
internal sealed class SyncCartsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.benefits.v1.BenefitPlanService.</summary>
internal sealed class SyncCartsIdBenefitPlanTarget(ApiBenefits.BenefitPlanService.BenefitPlanServiceClient client) : ISyncCartsIdDestination
{
    public async Task DeliverAsync(SyncCartsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBenefits.CreateBenefitPlanRequest { BenefitPlan = new ApiBenefits.BenefitPlan { BenefitPlanId = record.CorrelationId } }, token, client.CreateBenefitPlanAsync);
    }
}

/// <summary>Hands records to acme.itsm.v1.ServiceAssetReportingService.</summary>
internal sealed class SyncCartsIdServiceAssetReportingWriter : ISyncCartsIdDestination
{
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _client;

    public SyncCartsIdServiceAssetReportingWriter(ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncCartsIdRecord record, CancellationToken token)
    {
        await _client.GenerateServiceAssetReportAsync(new ApiItsm.GenerateServiceAssetReportRequest { Details = new ApiItsm.GenerateServiceAssetReportRequest.Types.Details { Cron = record.CorrelationId, Format = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingPlanService.</summary>
internal sealed class SyncCartsIdOnboardingPlanWriter : ISyncCartsIdDestination
{
    private readonly ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient _client;

    public SyncCartsIdOnboardingPlanWriter(ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncCartsIdRecord record, CancellationToken token)
    {
        await _client.CloneOnboardingPlanAsync(new ApiOnboarding.CloneOnboardingPlanRequest { RequestId = record.CorrelationId, OnboardingPlan = new ApiOnboarding.OnboardingPlan { Notes = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.DeskCalculationService.</summary>
internal sealed class SyncCartsIdDeskCalculationWriter(ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient client) : ISyncCartsIdDestination
{
    public async Task DeliverAsync(SyncCartsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.RecalculateDesksRequest { Details = new ApiWorkplaces.RecalculateDesksRequest.Types.Details { CurrencyCode = record.Status } }, token, client.RecalculateDesksAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncCartsIdLogOnly : ISyncCartsIdDestination
{
    public Task DeliverAsync(SyncCartsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.cart.v1.CartService/ListCarts and hands what it keeps to the sink of its kind.</summary>
internal static class SyncCartsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCart.CartService.CartServiceClient(channel);
        var reply = await source.ListCartsAsync(new ApiCart.ListCartsRequest() { OrderBy = "primary-sync-carts-id" }, cancellationToken: token);
        var record = new SyncCartsIdRecord
        {
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
            Status = reply.Result.Status,
        };
        await RouteSyncCartsIdAsync("onboarding-plan", record, channel, token);
    }

    private static async Task RouteSyncCartsIdAsync(string kind, SyncCartsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISyncCartsIdDestination sink = kind switch
        {
            "benefit-plan" => new SyncCartsIdBenefitPlanTarget(new ApiBenefits.BenefitPlanService.BenefitPlanServiceClient(channel)),
            "asset-reporting" => new SyncCartsIdServiceAssetReportingWriter(new ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient(channel)),
            "onboarding-plan" => new SyncCartsIdOnboardingPlanWriter(new ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient(channel)),
            "desk-calculation" => new SyncCartsIdDeskCalculationWriter(new ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient(channel)),
            _ => new SyncCartsIdLogOnly(),
        };
        await sink.DeliverAsync(record, token);
    }
}
