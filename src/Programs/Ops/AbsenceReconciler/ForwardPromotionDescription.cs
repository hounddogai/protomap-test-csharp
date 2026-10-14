using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where ForwardPromotionDescription hands each record.</summary>
internal interface IForwardPromotionDescriptionWriter
{
    Task DeliverAsync(ForwardPromotionDescriptionRecord record, CancellationToken token);
}

/// <summary>What ForwardPromotionDescription keeps of each ClonePromotion response.</summary>
internal sealed class ForwardPromotionDescriptionRecord
{
    public string Description { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Writes records to acme.benefits.v1.DependentValidationService.</summary>
internal sealed class ForwardPromotionDescriptionDependentValidationTarget : IForwardPromotionDescriptionWriter
{
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _client;

    public ForwardPromotionDescriptionDependentValidationTarget(ApiBenefits.DependentValidationService.DependentValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(ForwardPromotionDescriptionRecord record, CancellationToken token)
    {
        await _client.CheckDependentEligibilityAsync(new ApiBenefits.CheckDependentEligibilityRequest { DependentId = record.Name, Dependent = new ApiBenefits.Dependent { DependentId = record.Description } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.timekeeping.v1.ClockEventApprovalService.</summary>
internal sealed class ForwardPromotionDescriptionClockEventApprovalSink(ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient client) : IForwardPromotionDescriptionWriter
{
    public async Task DeliverAsync(ForwardPromotionDescriptionRecord record, CancellationToken token)
    {
        await client.CommentClockEventApprovalAsync(new ApiTimekeeping.CommentClockEventApprovalRequest { ClockEvent = new ApiTimekeeping.ClockEvent { ClockEventId = record.Name, Kind = record.Description } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.concessions.v1.ConcessionStandQueryService.</summary>
internal sealed class ForwardPromotionDescriptionConcessionStandQueryRelay : IForwardPromotionDescriptionWriter
{
    private readonly ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient _client;

    public ForwardPromotionDescriptionConcessionStandQueryRelay(ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(ForwardPromotionDescriptionRecord record, CancellationToken token)
    {
        await _client.SuggestConcessionStandsAsync(new ApiConcessions.SuggestConcessionStandsRequest { Filter = new ApiConcessions.SuggestConcessionStandsRequest.Types.Filter { OwnerId = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.promotions.v1.PromotionService/ClonePromotion and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardPromotionDescription
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPromotions.PromotionService.PromotionServiceClient(channel);
        var reply = await source.ClonePromotionAsync(new ApiPromotions.ClonePromotionRequest() { RequestId = "default-forward-promotion-description" }, cancellationToken: token);
        var record = new ForwardPromotionDescriptionRecord
        {
            Description = Text.Normalize(reply.Promotion.Description),
            Name = reply.Promotion.Name,
        };
        var sinks = new IForwardPromotionDescriptionWriter[]
        {
            new ForwardPromotionDescriptionDependentValidationTarget(new ApiBenefits.DependentValidationService.DependentValidationServiceClient(channel)),
            new ForwardPromotionDescriptionClockEventApprovalSink(new ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient(channel)),
            new ForwardPromotionDescriptionConcessionStandQueryRelay(new ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient(channel)),
        };
        await sinks[2].DeliverAsync(record, token);
    }
}
