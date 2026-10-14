using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiChat = Acme.Chat.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where BackfillChatRoomMessage hands each record.</summary>
internal interface IBackfillChatRoomMessageTarget
{
    Task ApplyAsync(BackfillChatRoomMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillChatRoomMessage keeps of each UpdateChatRoom response.</summary>
internal sealed class BackfillChatRoomMessageRecord
{
    public string Message { get; init; } = "";
    public string ChatRoomId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.corporateevents.v1.VenueValidationService.</summary>
internal sealed class BackfillChatRoomMessageVenueValidationSink : IBackfillChatRoomMessageTarget
{
    private readonly ApiCorporateevents.VenueValidationService.VenueValidationServiceClient _client;

    public BackfillChatRoomMessageVenueValidationSink(ApiCorporateevents.VenueValidationService.VenueValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillChatRoomMessageRecord record, CancellationToken token)
    {
        await _client.ScoreVenueAsync(new ApiCorporateevents.ScoreVenueRequest { Venue = new ApiCorporateevents.Venue { Code = record.Status, Name = record.ChatRoomId }, VenueId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.invoicing.v1.InvoiceBatchCalculationService.</summary>
internal sealed class BackfillChatRoomMessageInvoiceBatchCalculationRelay : IBackfillChatRoomMessageTarget
{
    private readonly ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient _client;

    public BackfillChatRoomMessageInvoiceBatchCalculationRelay(ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillChatRoomMessageRecord record, CancellationToken token)
    {
        await _client.RecalculateInvoiceBatchesAsync(new ApiInvoicing.RecalculateInvoiceBatchesRequest { InvoiceBatch = new ApiInvoicing.InvoiceBatch { Notes = record.Status, Number = record.ChatRoomId }, InvoiceBatchId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.licensing.v1.LicenseAssignmentValidationService.</summary>
internal sealed class BackfillChatRoomMessageLicenseAssignmentValidationWriter : IBackfillChatRoomMessageTarget
{
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _client;

    public BackfillChatRoomMessageLicenseAssignmentValidationWriter(ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillChatRoomMessageRecord record, CancellationToken token)
    {
        await _client.PreviewLicenseAssignmentAsync(new ApiLicensing.PreviewLicenseAssignmentRequest { LicenseAssignment = new ApiLicensing.LicenseAssignment { LicenseAssignmentId = record.Message, Number = record.Status }, LicenseAssignmentId = record.ChatRoomId }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employees.v1.EmployeeQueryService.</summary>
internal sealed class BackfillChatRoomMessageEmployeeQueryWriter(ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient client) : IBackfillChatRoomMessageTarget
{
    public async Task ApplyAsync(BackfillChatRoomMessageRecord record, CancellationToken token)
    {
        await client.SummarizeEmployeesAsync(new ApiEmployees.SummarizeEmployeesRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.chat.v1.ChatRoomService/UpdateChatRoom and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillChatRoomMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiChat.ChatRoomService.ChatRoomServiceClient(channel);
        var found = await source.UpdateChatRoomAsync(new ApiChat.UpdateChatRoomRequest(), cancellationToken: token);
        var record = new BackfillChatRoomMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            ChatRoomId = found.ChatRoom.ChatRoomId,
            Status = found.Result.Status,
        };
        await ActOnBackfillChatRoomMessageAsync("invoice-batch-calculation", record, channel, token);
    }

    private static async Task ActOnBackfillChatRoomMessageAsync(string kind, BackfillChatRoomMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["venue-validation"] = () => new BackfillChatRoomMessageVenueValidationSink(new ApiCorporateevents.VenueValidationService.VenueValidationServiceClient(channel)).ApplyAsync(record, token),
            ["invoice-batch-calculation"] = () => new BackfillChatRoomMessageInvoiceBatchCalculationRelay(new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(channel)).ApplyAsync(record, token),
            ["license-assignment-validation"] = () => new BackfillChatRoomMessageLicenseAssignmentValidationWriter(new ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient(channel)).ApplyAsync(record, token),
            ["employee-query"] = () => new BackfillChatRoomMessageEmployeeQueryWriter(new ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
