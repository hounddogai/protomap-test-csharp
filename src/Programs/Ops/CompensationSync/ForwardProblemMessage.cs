using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ForwardProblemMessage hands each record.</summary>
internal interface IForwardProblemMessageWriter
{
    Task ApplyAsync(ForwardProblemMessageRecord record, CancellationToken token);
}

/// <summary>What ForwardProblemMessage keeps of each CalculateProblem response.</summary>
internal sealed class ForwardProblemMessageRecord
{
    public string Message { get; init; } = "";
    public string ProblemId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.identity.v1.UserQueryService.</summary>
internal sealed class ForwardProblemMessageUserQuerySink(ApiIdentity.UserQueryService.UserQueryServiceClient client) : IForwardProblemMessageWriter
{
    public async Task ApplyAsync(ForwardProblemMessageRecord record, CancellationToken token)
    {
        await client.SuggestUsersAsync(new ApiIdentity.SuggestUsersRequest { Page = new ApiCommon.PageRequest { PageToken = record.ProblemId }, Filter = new ApiIdentity.SuggestUsersRequest.Types.Filter { Region = record.Status }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.ItineraryLifecycleService.</summary>
internal sealed class ForwardProblemMessageItineraryLifecycleRelay : IForwardProblemMessageWriter
{
    private readonly ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient _client;

    public ForwardProblemMessageItineraryLifecycleRelay(ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(ForwardProblemMessageRecord record, CancellationToken token)
    {
        await _client.TerminateItineraryAsync(new ApiTravel.TerminateItineraryRequest { Details = new ApiTravel.TerminateItineraryRequest.Types.Details { ReasonCode = record.ProblemId }, ItineraryId = record.Status, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.helpdesk.v1.EscalationValidationService.</summary>
internal sealed class ForwardProblemMessageEscalationValidationTarget(ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient client) : IForwardProblemMessageWriter
{
    public async Task ApplyAsync(ForwardProblemMessageRecord record, CancellationToken token)
    {
        await client.ValidateEscalationAsync(new ApiHelpdesk.ValidateEscalationRequest { Escalation = new ApiHelpdesk.Escalation { Subject = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ProblemCalculationService/CalculateProblem and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardProblemMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient(channel);
        var reply = await source.CalculateProblemAsync(new ApiItsm.CalculateProblemRequest() { Reason = "main-forward-problem-message" }, cancellationToken: token);
        var record = new ForwardProblemMessageRecord
        {
            Message = Text.Normalize(reply.Result.Message),
            ProblemId = reply.Problem.ProblemId,
            Status = reply.Result.Status,
        };
        var sinks = new Dictionary<string, IForwardProblemMessageWriter>
        {
            ["user-query"] = new ForwardProblemMessageUserQuerySink(new ApiIdentity.UserQueryService.UserQueryServiceClient(channel)),
            ["itinerary-lifecycle"] = new ForwardProblemMessageItineraryLifecycleRelay(new ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient(channel)),
            ["escalation-validation"] = new ForwardProblemMessageEscalationValidationTarget(new ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("FORWARD_PROBLEM_MESSAGE_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].ApplyAsync(record, token);
    }
}
