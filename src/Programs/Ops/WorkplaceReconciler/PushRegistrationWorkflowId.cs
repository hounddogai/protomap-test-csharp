using Grpc.Net.Client;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where PushRegistrationWorkflowId hands each record.</summary>
internal interface IPushRegistrationWorkflowIdTarget
{
    Task WriteAsync(PushRegistrationWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What PushRegistrationWorkflowId keeps of each InspectRegistrationWorkflow response.</summary>
internal sealed class PushRegistrationWorkflowIdRecord
{
    public string RegistrationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.loyalty.v1.LoyaltyMemberService.</summary>
internal sealed class PushRegistrationWorkflowIdLoyaltyMemberRelay : IPushRegistrationWorkflowIdTarget
{
    private readonly ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient _client;

    public PushRegistrationWorkflowIdLoyaltyMemberRelay(ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient client) => _client = client;

    public async Task WriteAsync(PushRegistrationWorkflowIdRecord record, CancellationToken token)
    {
        await _client.CreateLoyaltyMemberAsync(new ApiLoyalty.CreateLoyaltyMemberRequest { LoyaltyMember = new ApiLoyalty.LoyaltyMember { CurrencyCode = record.RegistrationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.learning.v1.CertificationWorkflowService.</summary>
internal sealed class PushRegistrationWorkflowIdCertificationWorkflowTarget : IPushRegistrationWorkflowIdTarget
{
    private readonly ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient _client;

    public PushRegistrationWorkflowIdCertificationWorkflowTarget(ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(PushRegistrationWorkflowIdRecord record, CancellationToken token)
    {
        await _client.StartCertificationWorkflowAsync(new ApiLearning.StartCertificationWorkflowRequest { CertificationId = record.Message, Certification = new ApiLearning.Certification { Number = record.RegistrationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.workplaces.v1.RoomBookingWorkflowService.</summary>
internal sealed class PushRegistrationWorkflowIdRoomBookingWorkflowSink(ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient client) : IPushRegistrationWorkflowIdTarget
{
    public async Task WriteAsync(PushRegistrationWorkflowIdRecord record, CancellationToken token)
    {
        await client.InspectRoomBookingWorkflowAsync(new ApiWorkplaces.InspectRoomBookingWorkflowRequest { RoomBookingId = record.RegistrationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.forecasting.v1.DemandForecastService.</summary>
internal sealed class PushRegistrationWorkflowIdDemandForecastTarget : IPushRegistrationWorkflowIdTarget
{
    private readonly ApiForecasting.DemandForecastService.DemandForecastServiceClient _client;

    public PushRegistrationWorkflowIdDemandForecastTarget(ApiForecasting.DemandForecastService.DemandForecastServiceClient client) => _client = client;

    public async Task WriteAsync(PushRegistrationWorkflowIdRecord record, CancellationToken token)
    {
        await _client.SearchDemandForecastsAsync(new ApiForecasting.SearchDemandForecastsRequest { OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushRegistrationWorkflowIdLogOnly : IPushRegistrationWorkflowIdTarget
{
    public Task WriteAsync(PushRegistrationWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.RegistrationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.corporateevents.v1.RegistrationWorkflowService/InspectRegistrationWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class PushRegistrationWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient(channel);
        var response = await source.InspectRegistrationWorkflowAsync(new ApiCorporateevents.InspectRegistrationWorkflowRequest() { RegistrationId = "nightly-push-registration-workflow-id" }, cancellationToken: token);
        var record = new PushRegistrationWorkflowIdRecord
        {
            RegistrationId = response.Registration.RegistrationId,
            Message = response.Result.Message,
        };
        await DispatchPushRegistrationWorkflowIdAsync("loyalty-member", record, channel, token);
    }

    private static async Task DispatchPushRegistrationWorkflowIdAsync(string kind, PushRegistrationWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<PushRegistrationWorkflowIdRecord, CancellationToken, Task> write = new PushRegistrationWorkflowIdLogOnly().WriteAsync;
        switch (kind)
        {
            case "loyalty-member":
                write = new PushRegistrationWorkflowIdLoyaltyMemberRelay(new ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient(channel)).WriteAsync;
                break;
            case "certification-workflow":
                write = new PushRegistrationWorkflowIdCertificationWorkflowTarget(new ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient(channel)).WriteAsync;
                break;
            case "room-booking-workflow":
                write = new PushRegistrationWorkflowIdRoomBookingWorkflowSink(new ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient(channel)).WriteAsync;
                break;
            case "demand-forecast":
                write = new PushRegistrationWorkflowIdDemandForecastTarget(new ApiForecasting.DemandForecastService.DemandForecastServiceClient(channel)).WriteAsync;
                break;
        }
        await write(record, token);
    }
}
