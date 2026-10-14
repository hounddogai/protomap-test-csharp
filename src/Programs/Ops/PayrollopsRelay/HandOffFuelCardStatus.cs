using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiFuel = Acme.Fuel.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where HandOffFuelCardStatus hands each record.</summary>
internal interface IHandOffFuelCardStatusTarget
{
    Task WriteAsync(HandOffFuelCardStatusRecord record, CancellationToken token);
}

/// <summary>What HandOffFuelCardStatus keeps of each SimulateFuelCard response.</summary>
internal sealed class HandOffFuelCardStatusRecord
{
    public string Status { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string Message { get; init; } = "";
    public string FuelCardId { get; init; } = "";
}

/// <summary>Passes records to acme.recruiting.v1.InterviewApprovalService.</summary>
internal sealed class HandOffFuelCardStatusInterviewApprovalSink : IHandOffFuelCardStatusTarget
{
    private readonly ApiRecruiting.InterviewApprovalService.InterviewApprovalServiceClient _client;

    public HandOffFuelCardStatusInterviewApprovalSink(ApiRecruiting.InterviewApprovalService.InterviewApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffFuelCardStatusRecord record, CancellationToken token)
    {
        await _client.EscalateInterviewAsync(new ApiRecruiting.EscalateInterviewRequest { InterviewId = record.Status, Interview = new ApiRecruiting.Interview { InterviewId = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.ReviewWorkflowService.</summary>
internal sealed class HandOffFuelCardStatusReviewWorkflowSink(ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient client) : IHandOffFuelCardStatusTarget
{
    public async Task WriteAsync(HandOffFuelCardStatusRecord record, CancellationToken token)
    {
        await client.PauseReviewWorkflowAsync(new ApiPerformance.PauseReviewWorkflowRequest { Reason = record.Status, Review = new ApiPerformance.Review { ReviewId = record.FuelCardId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.fuel.v1.FuelCardCalculationService/SimulateFuelCard and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffFuelCardStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient(channel);
        var reply = await source.SimulateFuelCardAsync(new ApiFuel.SimulateFuelCardRequest() { Reason = "default-hand-off-fuel-card-status" }, cancellationToken: token);
        var record = new HandOffFuelCardStatusRecord
        {
            Status = Text.Normalize(reply.Result.Status),
            CurrencyCode = reply.FuelCard.CurrencyCode,
            Message = reply.Result.Message,
            FuelCardId = reply.FuelCard.FuelCardId,
        };
        var sinks = new Dictionary<string, IHandOffFuelCardStatusTarget>
        {
            ["interview-approval"] = new HandOffFuelCardStatusInterviewApprovalSink(new ApiRecruiting.InterviewApprovalService.InterviewApprovalServiceClient(channel)),
            ["review-workflow"] = new HandOffFuelCardStatusReviewWorkflowSink(new ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient(channel)),
        };
        await RouteHandOffFuelCardStatusAsync("interview-approval", record, sinks, token);
    }

    private static async Task RouteHandOffFuelCardStatusAsync(string kind, HandOffFuelCardStatusRecord record, IReadOnlyDictionary<string, IHandOffFuelCardStatusTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
