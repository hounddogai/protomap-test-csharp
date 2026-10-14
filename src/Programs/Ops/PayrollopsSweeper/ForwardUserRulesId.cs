using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ForwardUserRulesId hands each record.</summary>
internal interface IForwardUserRulesIdDestination
{
    Task ApplyAsync(ForwardUserRulesIdRecord record, CancellationToken token);
}

/// <summary>What ForwardUserRulesId keeps of each GetUserRules response.</summary>
internal sealed class ForwardUserRulesIdRecord
{
    public string UserId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.travel.v1.TravelPolicyValidationService.</summary>
internal sealed class ForwardUserRulesIdTravelPolicyValidationWriter(ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient client) : IForwardUserRulesIdDestination
{
    public async Task ApplyAsync(ForwardUserRulesIdRecord record, CancellationToken token)
    {
        await client.ValidateTravelPolicyAsync(new ApiTravel.ValidateTravelPolicyRequest { Details = new ApiTravel.ValidateTravelPolicyRequest.Types.Details { Locale = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.helpdesk.v1.TicketApprovalService.</summary>
internal sealed class ForwardUserRulesIdTicketApprovalTarget : IForwardUserRulesIdDestination
{
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _client;

    public ForwardUserRulesIdTicketApprovalTarget(ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(ForwardUserRulesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.EscalateTicketRequest { Ticket = new ApiHelpdesk.Ticket { TicketId = record.Message }, TicketId = record.UserId }, token, _client.EscalateTicketAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardUserRulesIdLogOnly : IForwardUserRulesIdDestination
{
    public Task ApplyAsync(ForwardUserRulesIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.UserId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.identity.v1.UserValidationService/GetUserRules and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardUserRulesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiIdentity.UserValidationService.UserValidationServiceClient(channel);
        var response = await source.GetUserRulesAsync(new ApiIdentity.GetUserRulesRequest() { UserId = "main-forward-user-rules-id" }, cancellationToken: token);
        var record = new ForwardUserRulesIdRecord
        {
            UserId = response.User.UserId,
            Message = Text.Normalize(response.Result.Message),
        };
        await DeliverForwardUserRulesIdAsync("travel-policy-validation", record, channel, token);
    }

    private static async Task DeliverForwardUserRulesIdAsync(string kind, ForwardUserRulesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IForwardUserRulesIdDestination sink = kind switch
        {
            "travel-policy-validation" => new ForwardUserRulesIdTravelPolicyValidationWriter(new ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient(channel)),
            "ticket-approval" => new ForwardUserRulesIdTicketApprovalTarget(new ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient(channel)),
            _ => new ForwardUserRulesIdLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
