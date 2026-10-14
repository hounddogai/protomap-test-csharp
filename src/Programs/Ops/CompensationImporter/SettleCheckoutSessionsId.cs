using Grpc.Net.Client;
using ApiCheckout = Acme.Checkout.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where SettleCheckoutSessionsId hands each record.</summary>
internal interface ISettleCheckoutSessionsIdTarget
{
    Task SendAsync(SettleCheckoutSessionsIdRecord record, CancellationToken token);
}

/// <summary>What SettleCheckoutSessionsId keeps of each CountCheckoutSessions response.</summary>
internal sealed class SettleCheckoutSessionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningCalculationService.</summary>
internal sealed class SettleCheckoutSessionsIdJobOpeningCalculationWriter : ISettleCheckoutSessionsIdTarget
{
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _client;

    public SettleCheckoutSessionsIdJobOpeningCalculationWriter(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) => _client = client;

    public async Task SendAsync(SettleCheckoutSessionsIdRecord record, CancellationToken token)
    {
        await _client.CompareJobOpeningOptionsAsync(new ApiRecruiting.CompareJobOpeningOptionsRequest { Details = new ApiRecruiting.CompareJobOpeningOptionsRequest.Types.Details { CurrencyCode = record.Message }, JobOpening = new ApiRecruiting.JobOpening { JobOpeningId = record.Status, Number = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.workplaces.v1.BuildingService.</summary>
internal sealed class SettleCheckoutSessionsIdBuildingWriter(ApiWorkplaces.BuildingService.BuildingServiceClient client) : ISettleCheckoutSessionsIdTarget
{
    public async Task SendAsync(SettleCheckoutSessionsIdRecord record, CancellationToken token)
    {
        await client.SearchBuildingsAsync(new ApiWorkplaces.SearchBuildingsRequest { Filter = new ApiWorkplaces.SearchBuildingsRequest.Types.Filter { Region = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.learning.v1.CertificationCalculationService.</summary>
internal sealed class SettleCheckoutSessionsIdCertificationCalculationSink(ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient client) : ISettleCheckoutSessionsIdTarget
{
    public async Task SendAsync(SettleCheckoutSessionsIdRecord record, CancellationToken token)
    {
        await client.SimulateCertificationAsync(new ApiLearning.SimulateCertificationRequest { CertificationId = record.Message, Certification = new ApiLearning.Certification { Notes = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.loyalty.v1.LoyaltyMemberQueryService.</summary>
internal sealed class SettleCheckoutSessionsIdLoyaltyMemberQuerySink : ISettleCheckoutSessionsIdTarget
{
    private readonly ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient _client;

    public SettleCheckoutSessionsIdLoyaltyMemberQuerySink(ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient client) => _client = client;

    public async Task SendAsync(SettleCheckoutSessionsIdRecord record, CancellationToken token)
    {
        await _client.PeekLoyaltyMemberAsync(new ApiLoyalty.PeekLoyaltyMemberRequest { LoyaltyMemberId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.checkout.v1.CheckoutSessionQueryService/CountCheckoutSessions and hands what it keeps to the sink of its kind.</summary>
internal static class SettleCheckoutSessionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient(channel);
        var reply = await source.CountCheckoutSessionsAsync(new ApiCheckout.CountCheckoutSessionsRequest() { OrderBy = "default-settle-checkout-sessions-id" }, cancellationToken: token);
        var record = new SettleCheckoutSessionsIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Message = reply.Result.Message,
            Status = reply.Result.Status,
        };
        await ActOnSettleCheckoutSessionsIdAsync("certification-calculation", record, channel, token);
    }

    private static async Task ActOnSettleCheckoutSessionsIdAsync(string kind, SettleCheckoutSessionsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["job-opening-calculation"] = () => new SettleCheckoutSessionsIdJobOpeningCalculationWriter(new ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient(channel)).SendAsync(record, token),
            ["building"] = () => new SettleCheckoutSessionsIdBuildingWriter(new ApiWorkplaces.BuildingService.BuildingServiceClient(channel)).SendAsync(record, token),
            ["certification-calculation"] = () => new SettleCheckoutSessionsIdCertificationCalculationSink(new ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient(channel)).SendAsync(record, token),
            ["loyalty-member-query"] = () => new SettleCheckoutSessionsIdLoyaltyMemberQuerySink(new ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient(channel)).SendAsync(record, token),
        };
        await actions[kind]();
    }
}
