using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAudiences = Acme.Audiences.V1;
using ApiBanking = Acme.Banking.V1;
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where CarrySoftwareLicensesId hands each record.</summary>
internal interface ICarrySoftwareLicensesIdSink
{
    Task SendAsync(CarrySoftwareLicensesIdRecord record, CancellationToken token);
}

/// <summary>What CarrySoftwareLicensesId keeps of each SuggestSoftwareLicenses response.</summary>
internal sealed class CarrySoftwareLicensesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Hands records to acme.performance.v1.FeedbackApprovalService.</summary>
internal sealed class CarrySoftwareLicensesIdFeedbackApprovalSink(ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient client) : ICarrySoftwareLicensesIdSink
{
    public async Task SendAsync(CarrySoftwareLicensesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.SubmitFeedbackRequest { Feedback = new ApiPerformance.Feedback { CorrelationId = record.NextPageToken }, Details = new ApiPerformance.SubmitFeedbackRequest.Types.Details { Comment = record.CorrelationId } }, token, client.SubmitFeedbackAsync);
    }
}

/// <summary>Hands records to acme.chat.v1.ChatRoomCalculationService.</summary>
internal sealed class CarrySoftwareLicensesIdChatRoomCalculationSink : ICarrySoftwareLicensesIdSink
{
    private readonly ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient _client;

    public CarrySoftwareLicensesIdChatRoomCalculationSink(ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient client) => _client = client;

    public async Task SendAsync(CarrySoftwareLicensesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiChat.SimulateChatRoomRequest { ChatRoom = new ApiChat.ChatRoom { Name = record.CorrelationId }, ChatRoomId = record.NextPageToken }, token, _client.SimulateChatRoomAsync);
    }
}

/// <summary>Writes records to acme.banking.v1.BankAccountService.</summary>
internal sealed class CarrySoftwareLicensesIdBankAccountSink(ApiBanking.BankAccountService.BankAccountServiceClient client) : ICarrySoftwareLicensesIdSink
{
    public async Task SendAsync(CarrySoftwareLicensesIdRecord record, CancellationToken token)
    {
        await client.CreateBankAccountAsync(new ApiBanking.CreateBankAccountRequest { RequestId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.audiences.v1.AudienceService.</summary>
internal sealed class CarrySoftwareLicensesIdAudienceSink(ApiAudiences.AudienceService.AudienceServiceClient client) : ICarrySoftwareLicensesIdSink
{
    public async Task SendAsync(CarrySoftwareLicensesIdRecord record, CancellationToken token)
    {
        await client.SearchAudiencesAsync(new ApiAudiences.SearchAudiencesRequest { Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CarrySoftwareLicensesIdLogOnly : ICarrySoftwareLicensesIdSink
{
    public Task SendAsync(CarrySoftwareLicensesIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.licensing.v1.SoftwareLicenseQueryService/SuggestSoftwareLicenses and hands what it keeps to the sink of its kind.</summary>
internal static class CarrySoftwareLicensesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient(channel);
        var got = await source.SuggestSoftwareLicensesAsync(new ApiLicensing.SuggestSoftwareLicensesRequest() { OrderBy = "current-carry-software-licenses-id" }, cancellationToken: token);
        var record = new CarrySoftwareLicensesIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            NextPageToken = Text.Normalize(got.PageInfo.NextPageToken),
        };
        await DeliverCarrySoftwareLicensesIdAsync("audience", record, channel, token);
    }

    private static async Task DeliverCarrySoftwareLicensesIdAsync(string kind, CarrySoftwareLicensesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ICarrySoftwareLicensesIdSink sink = new CarrySoftwareLicensesIdLogOnly();
        switch (kind)
        {
            case "feedback-approval":
                sink = new CarrySoftwareLicensesIdFeedbackApprovalSink(new ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient(channel));
                break;
            case "chat-room-calculation":
                sink = new CarrySoftwareLicensesIdChatRoomCalculationSink(new ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient(channel));
                break;
            case "bank-account":
                sink = new CarrySoftwareLicensesIdBankAccountSink(new ApiBanking.BankAccountService.BankAccountServiceClient(channel));
                break;
            case "audience":
                sink = new CarrySoftwareLicensesIdAudienceSink(new ApiAudiences.AudienceService.AudienceServiceClient(channel));
                break;
        }
        await sink.SendAsync(record, token);
    }
}
