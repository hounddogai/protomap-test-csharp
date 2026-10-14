using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where SettleJobOpeningEligibilityId hands each record.</summary>
internal interface ISettleJobOpeningEligibilityIdSink
{
    Task SendAsync(SettleJobOpeningEligibilityIdRecord record, CancellationToken token);
}

/// <summary>What SettleJobOpeningEligibilityId keeps of each CheckJobOpeningEligibility response.</summary>
internal sealed class SettleJobOpeningEligibilityIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Number { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.benefits.v1.DependentValidationService.</summary>
internal sealed class SettleJobOpeningEligibilityIdDependentValidationRelay(ApiBenefits.DependentValidationService.DependentValidationServiceClient client) : ISettleJobOpeningEligibilityIdSink
{
    public async Task SendAsync(SettleJobOpeningEligibilityIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBenefits.NormalizeDependentRequest { Reason = record.Status, Details = new ApiBenefits.NormalizeDependentRequest.Types.Details { Locale = record.Number } }, token, client.NormalizeDependentAsync);
    }
}

/// <summary>Sends records to acme.performance.v1.GoalApprovalService.</summary>
internal sealed class SettleJobOpeningEligibilityIdGoalApprovalTarget : ISettleJobOpeningEligibilityIdSink
{
    private readonly ApiPerformance.GoalApprovalService.GoalApprovalServiceClient _client;

    public SettleJobOpeningEligibilityIdGoalApprovalTarget(ApiPerformance.GoalApprovalService.GoalApprovalServiceClient client) => _client = client;

    public async Task SendAsync(SettleJobOpeningEligibilityIdRecord record, CancellationToken token)
    {
        await _client.ListPendingGoalsAsync(new ApiPerformance.ListPendingGoalsRequest { Filter = new ApiPerformance.ListPendingGoalsRequest.Types.Filter { OwnerId = record.Number }, OrderBy = record.Message, Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.recruiting.v1.JobOpeningValidationService/CheckJobOpeningEligibility and hands what it keeps to the sink of its kind.</summary>
internal static class SettleJobOpeningEligibilityId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient(channel);
        var reply = await source.CheckJobOpeningEligibilityAsync(new ApiRecruiting.CheckJobOpeningEligibilityRequest() { JobOpeningId = "primary-settle-job-opening-eligibility-id" }, cancellationToken: token);
        var record = new SettleJobOpeningEligibilityIdRecord
        {
            ReferenceId = reply.Outcome.ReferenceId,
            Number = reply.JobOpening.Number,
            Status = Text.Normalize(reply.Result.Status),
            Message = reply.Outcome.Message,
        };
        var sinks = new Dictionary<string, ISettleJobOpeningEligibilityIdSink>
        {
            ["dependent-validation"] = new SettleJobOpeningEligibilityIdDependentValidationRelay(new ApiBenefits.DependentValidationService.DependentValidationServiceClient(channel)),
            ["goal-approval"] = new SettleJobOpeningEligibilityIdGoalApprovalTarget(new ApiPerformance.GoalApprovalService.GoalApprovalServiceClient(channel)),
        };
        await RouteSettleJobOpeningEligibilityIdAsync("goal-approval", record, sinks, token);
    }

    private static async Task RouteSettleJobOpeningEligibilityIdAsync(string kind, SettleJobOpeningEligibilityIdRecord record, IReadOnlyDictionary<string, ISettleJobOpeningEligibilityIdSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
