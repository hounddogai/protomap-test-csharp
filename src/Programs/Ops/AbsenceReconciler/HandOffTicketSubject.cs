using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLedger = Acme.Ledger.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where HandOffTicketSubject hands each record.</summary>
internal interface IHandOffTicketSubjectSink
{
    Task WriteAsync(HandOffTicketSubjectRecord record, CancellationToken token);
}

/// <summary>What HandOffTicketSubject keeps of each CreateTicket response.</summary>
internal sealed class HandOffTicketSubjectRecord
{
    public string Subject { get; init; } = "";
    public string Message { get; init; } = "";
    public string TicketId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.documents.v1.DocumentFolderValidationService.</summary>
internal sealed class HandOffTicketSubjectDocumentFolderValidationWriter(ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient client) : IHandOffTicketSubjectSink
{
    public async Task WriteAsync(HandOffTicketSubjectRecord record, CancellationToken token)
    {
        await client.NormalizeDocumentFolderAsync(new ApiDocuments.NormalizeDocumentFolderRequest { Reason = record.Subject, DocumentFolder = new ApiDocuments.DocumentFolder { Name = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.ledger.v1.LedgerAccountValidationService.</summary>
internal sealed class HandOffTicketSubjectLedgerAccountValidationTarget : IHandOffTicketSubjectSink
{
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _client;

    public HandOffTicketSubjectLedgerAccountValidationTarget(ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffTicketSubjectRecord record, CancellationToken token)
    {
        await _client.CheckLedgerAccountEligibilityAsync(new ApiLedger.CheckLedgerAccountEligibilityRequest { LedgerAccount = new ApiLedger.LedgerAccount { LedgerAccountId = record.TicketId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffTicketSubjectLogOnly : IHandOffTicketSubjectSink
{
    public Task WriteAsync(HandOffTicketSubjectRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Subject}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.TicketService/CreateTicket and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffTicketSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.TicketService.TicketServiceClient(channel);
        var found = await source.CreateTicketAsync(new ApiHelpdesk.CreateTicketRequest() { RequestId = "default-hand-off-ticket-subject" }, cancellationToken: token);
        var record = new HandOffTicketSubjectRecord
        {
            Subject = Text.Normalize(found.Ticket.Subject),
            Message = found.Result.Message,
            TicketId = found.Ticket.TicketId,
            Status = found.Result.Status,
        };
        await DispatchHandOffTicketSubjectAsync("document-folder-validation", record, channel, token);
    }

    private static async Task DispatchHandOffTicketSubjectAsync(string kind, HandOffTicketSubjectRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<HandOffTicketSubjectRecord, CancellationToken, Task> write = new HandOffTicketSubjectLogOnly().WriteAsync;
        switch (kind)
        {
            case "document-folder-validation":
                write = new HandOffTicketSubjectDocumentFolderValidationWriter(new ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient(channel)).WriteAsync;
                break;
            case "ledger-account-validation":
                write = new HandOffTicketSubjectLedgerAccountValidationTarget(new ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient(channel)).WriteAsync;
                break;
        }
        await write(record, token);
    }
}
