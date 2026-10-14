using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAudiences = Acme.Audiences.V1;
using ApiCheckout = Acme.Checkout.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ReplayAudiencesId hands each record.</summary>
internal interface IReplayAudiencesIdTarget
{
    Task PushAsync(ReplayAudiencesIdRecord record, CancellationToken token);
}

/// <summary>What ReplayAudiencesId keeps of each ResolveAudiences response.</summary>
internal sealed class ReplayAudiencesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.recruiting.v1.InterviewReportingService.</summary>
internal sealed class ReplayAudiencesIdInterviewReportingSink : IReplayAudiencesIdTarget
{
    private readonly ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient _client;

    public ReplayAudiencesIdInterviewReportingSink(ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient client) => _client = client;

    public async Task PushAsync(ReplayAudiencesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.DeleteInterviewReportRequest { InterviewId = record.CorrelationId }, token, _client.DeleteInterviewReportAsync);
    }
}

/// <summary>Writes records to acme.fleet.v1.VehicleService.</summary>
internal sealed class ReplayAudiencesIdVehicleWriter : IReplayAudiencesIdTarget
{
    private readonly ApiFleet.VehicleService.VehicleServiceClient _client;

    public ReplayAudiencesIdVehicleWriter(ApiFleet.VehicleService.VehicleServiceClient client) => _client = client;

    public async Task PushAsync(ReplayAudiencesIdRecord record, CancellationToken token)
    {
        await _client.SearchVehiclesAsync(new ApiFleet.SearchVehiclesRequest { Filter = new ApiFleet.SearchVehiclesRequest.Types.Filter { Region = record.Status, Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.checkout.v1.CheckoutSessionQueryService.</summary>
internal sealed class ReplayAudiencesIdCheckoutSessionQueryWriter : IReplayAudiencesIdTarget
{
    private readonly ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient _client;

    public ReplayAudiencesIdCheckoutSessionQueryWriter(ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient client) => _client = client;

    public async Task PushAsync(ReplayAudiencesIdRecord record, CancellationToken token)
    {
        await _client.FindCheckoutSessionsAsync(new ApiCheckout.FindCheckoutSessionsRequest { Filter = new ApiCheckout.FindCheckoutSessionsRequest.Types.Filter { Query = record.Status, OwnerId = record.Message, Region = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.audiences.v1.AudienceQueryService/ResolveAudiences and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayAudiencesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudiences.AudienceQueryService.AudienceQueryServiceClient(channel);
        var got = await source.ResolveAudiencesAsync(new ApiAudiences.ResolveAudiencesRequest(), cancellationToken: token);
        var record = new ReplayAudiencesIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            NextPageToken = Text.Normalize(got.PageInfo.NextPageToken),
            Message = got.Result.Message,
            Status = got.Result.Status,
        };
        var sinks = new List<IReplayAudiencesIdTarget>
        {
            new ReplayAudiencesIdInterviewReportingSink(new ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient(channel)),
            new ReplayAudiencesIdVehicleWriter(new ApiFleet.VehicleService.VehicleServiceClient(channel)),
            new ReplayAudiencesIdCheckoutSessionQueryWriter(new ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient(channel)),
        };
        await sinks[0].PushAsync(record, token);
    }
}
