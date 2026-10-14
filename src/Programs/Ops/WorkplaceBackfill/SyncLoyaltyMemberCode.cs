using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAppointments = Acme.Appointments.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLoyalty = Acme.Loyalty.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where SyncLoyaltyMemberCode hands each record.</summary>
internal interface ISyncLoyaltyMemberCodeTarget
{
    Task DeliverAsync(SyncLoyaltyMemberCodeRecord record, CancellationToken token);
}

/// <summary>What SyncLoyaltyMemberCode keeps of each CalculateLoyaltyMember response.</summary>
internal sealed class SyncLoyaltyMemberCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string LoyaltyMemberId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.appointments.v1.AppointmentQueryService.</summary>
internal sealed class SyncLoyaltyMemberCodeAppointmentQueryTarget : ISyncLoyaltyMemberCodeTarget
{
    private readonly ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient _client;

    public SyncLoyaltyMemberCodeAppointmentQueryTarget(ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncLoyaltyMemberCodeRecord record, CancellationToken token)
    {
        await _client.SuggestAppointmentsAsync(new ApiAppointments.SuggestAppointmentsRequest { OrderBy = record.CurrencyCode, Filter = new ApiAppointments.SuggestAppointmentsRequest.Types.Filter { Query = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentFolderAdminService.</summary>
internal sealed class SyncLoyaltyMemberCodeDocumentFolderAdminSink(ApiDocuments.DocumentFolderAdminService.DocumentFolderAdminServiceClient client) : ISyncLoyaltyMemberCodeTarget
{
    public async Task DeliverAsync(SyncLoyaltyMemberCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.TransferDocumentFolderRequest { Details = new ApiDocuments.TransferDocumentFolderRequest.Types.Details { ReasonCode = record.LoyaltyMemberId }, DocumentFolderId = record.CurrencyCode }, token, client.TransferDocumentFolderAsync);
    }
}

/// <summary>Reads acme.loyalty.v1.LoyaltyMemberCalculationService/CalculateLoyaltyMember and hands what it keeps to the sink of its kind.</summary>
internal static class SyncLoyaltyMemberCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLoyalty.LoyaltyMemberCalculationService.LoyaltyMemberCalculationServiceClient(channel);
        var reply = await source.CalculateLoyaltyMemberAsync(new ApiLoyalty.CalculateLoyaltyMemberRequest() { Reason = "current-sync-loyalty-member-code" }, cancellationToken: token);
        var record = new SyncLoyaltyMemberCodeRecord
        {
            CurrencyCode = reply.LoyaltyMember.CurrencyCode,
            CorrelationId = reply.Result.CorrelationId,
            LoyaltyMemberId = Text.Normalize(reply.LoyaltyMember.LoyaltyMemberId),
            Status = reply.Result.Status,
        };
        await ActOnSyncLoyaltyMemberCodeAsync("appointment-query", record, channel, token);
    }

    private static async Task ActOnSyncLoyaltyMemberCodeAsync(string kind, SyncLoyaltyMemberCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["appointment-query"] = () => new SyncLoyaltyMemberCodeAppointmentQueryTarget(new ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient(channel)).DeliverAsync(record, token),
            ["document-folder-admin"] = () => new SyncLoyaltyMemberCodeDocumentFolderAdminSink(new ApiDocuments.DocumentFolderAdminService.DocumentFolderAdminServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
