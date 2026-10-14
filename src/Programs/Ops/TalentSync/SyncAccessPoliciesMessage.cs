using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAuthorization = Acme.Authorization.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiTicketing = Acme.Ticketing.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where SyncAccessPoliciesMessage hands each record.</summary>
internal interface ISyncAccessPoliciesMessageDestination
{
    Task DeliverAsync(SyncAccessPoliciesMessageRecord record, CancellationToken token);
}

/// <summary>What SyncAccessPoliciesMessage keeps of each LintAccessPolicies response.</summary>
internal sealed class SyncAccessPoliciesMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string ResultStatus { get; init; } = "";
}

/// <summary>Writes records to acme.employeeportal.v1.SelfServiceRequestIntegrationService.</summary>
internal sealed class SyncAccessPoliciesMessageSelfServiceRequestIntegrationRelay : ISyncAccessPoliciesMessageDestination
{
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _client;

    public SyncAccessPoliciesMessageSelfServiceRequestIntegrationRelay(ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncAccessPoliciesMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest { Details = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest.Types.Details { SourceSystem = record.Message }, Reason = record.ResultStatus }, token, _client.DisconnectSelfServiceRequestSourceAsync);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentTemplateNotificationService.</summary>
internal sealed class SyncAccessPoliciesMessageDocumentTemplateNotificationTarget : ISyncAccessPoliciesMessageDestination
{
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _client;

    public SyncAccessPoliciesMessageDocumentTemplateNotificationTarget(ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncAccessPoliciesMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest { Reason = record.Status }, token, _client.UnsubscribeDocumentTemplateUpdatesAsync);
    }
}

/// <summary>Hands records to acme.ticketing.v1.TicketService.</summary>
internal sealed class SyncAccessPoliciesMessageTicketSink(ApiTicketing.TicketService.TicketServiceClient client) : ISyncAccessPoliciesMessageDestination
{
    public async Task DeliverAsync(SyncAccessPoliciesMessageRecord record, CancellationToken token)
    {
        await client.UpdateTicketAsync(new ApiTicketing.UpdateTicketRequest { Ticket = new ApiTicketing.Ticket { TicketId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncAccessPoliciesMessageLogOnly : ISyncAccessPoliciesMessageDestination
{
    public Task DeliverAsync(SyncAccessPoliciesMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.authorization.v1.AccessPolicyValidationService/LintAccessPolicies and hands what it keeps to the sink of its kind.</summary>
internal static class SyncAccessPoliciesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient(channel);
        var found = await source.LintAccessPoliciesAsync(new ApiAuthorization.LintAccessPoliciesRequest() { Reason = "live-sync-access-policies-message" }, cancellationToken: token);
        var record = new SyncAccessPoliciesMessageRecord
        {
            Message = found.Outcome.Message,
            Status = found.Outcome.Status,
            ResultStatus = Text.Normalize(found.Result.Status),
        };
        await DispatchSyncAccessPoliciesMessageAsync("ticket", record, channel, token);
    }

    private static async Task DispatchSyncAccessPoliciesMessageAsync(string kind, SyncAccessPoliciesMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SyncAccessPoliciesMessageRecord, CancellationToken, Task> write = new SyncAccessPoliciesMessageLogOnly().DeliverAsync;
        switch (kind)
        {
            case "self-request-integration":
                write = new SyncAccessPoliciesMessageSelfServiceRequestIntegrationRelay(new ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient(channel)).DeliverAsync;
                break;
            case "document-template-notification":
                write = new SyncAccessPoliciesMessageDocumentTemplateNotificationTarget(new ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient(channel)).DeliverAsync;
                break;
            case "ticket":
                write = new SyncAccessPoliciesMessageTicketSink(new ApiTicketing.TicketService.TicketServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
