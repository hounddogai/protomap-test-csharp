using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAdmissions = Acme.Admissions.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where BackfillDispatchOrderStatus hands each record.</summary>
internal interface IBackfillDispatchOrderStatusSink
{
    Task PushAsync(BackfillDispatchOrderStatusRecord record, CancellationToken token);
}

/// <summary>What BackfillDispatchOrderStatus keeps of each DeleteDispatchOrder response.</summary>
internal sealed class BackfillDispatchOrderStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.travel.v1.TravelBookingValidationService.</summary>
internal sealed class BackfillDispatchOrderStatusTravelBookingValidationTarget : IBackfillDispatchOrderStatusSink
{
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _client;

    public BackfillDispatchOrderStatusTravelBookingValidationTarget(ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient client) => _client = client;

    public async Task PushAsync(BackfillDispatchOrderStatusRecord record, CancellationToken token)
    {
        await _client.PreviewTravelBookingAsync(new ApiTravel.PreviewTravelBookingRequest { Reason = record.Status, TravelBookingId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.admissions.v1.AdmissionService.</summary>
internal sealed class BackfillDispatchOrderStatusAdmissionTarget : IBackfillDispatchOrderStatusSink
{
    private readonly ApiAdmissions.AdmissionService.AdmissionServiceClient _client;

    public BackfillDispatchOrderStatusAdmissionTarget(ApiAdmissions.AdmissionService.AdmissionServiceClient client) => _client = client;

    public async Task PushAsync(BackfillDispatchOrderStatusRecord record, CancellationToken token)
    {
        await _client.SearchAdmissionsAsync(new ApiAdmissions.SearchAdmissionsRequest { OrderBy = record.CorrelationId, Filter = new ApiAdmissions.SearchAdmissionsRequest.Types.Filter { OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.recruiting.v1.CandidateCalculationService.</summary>
internal sealed class BackfillDispatchOrderStatusCandidateCalculationTarget : IBackfillDispatchOrderStatusSink
{
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _client;

    public BackfillDispatchOrderStatusCandidateCalculationTarget(ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient client) => _client = client;

    public async Task PushAsync(BackfillDispatchOrderStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.GetCandidateRatesRequest { CandidateId = record.CorrelationId }, token, _client.GetCandidateRatesAsync);
    }
}

/// <summary>Passes records to acme.itsm.v1.ProblemApprovalService.</summary>
internal sealed class BackfillDispatchOrderStatusProblemApprovalTarget(ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient client) : IBackfillDispatchOrderStatusSink
{
    public async Task PushAsync(BackfillDispatchOrderStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.SubmitProblemRequest { Details = new ApiItsm.SubmitProblemRequest.Types.Details { ApproverEmail = record.CorrelationId }, Problem = new ApiItsm.Problem { Subject = record.Status } }, token, client.SubmitProblemAsync);
    }
}

/// <summary>Reads acme.dispatch.v1.DispatchOrderService/DeleteDispatchOrder and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillDispatchOrderStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDispatch.DispatchOrderService.DispatchOrderServiceClient(channel);
        var found = await source.DeleteDispatchOrderAsync(new ApiDispatch.DeleteDispatchOrderRequest() { Etag = "main-backfill-dispatch-order-status" }, cancellationToken: token);
        var record = new BackfillDispatchOrderStatusRecord
        {
            Status = found.Result.Status,
            CorrelationId = found.Result.CorrelationId,
        };
        var sinks = new List<IBackfillDispatchOrderStatusSink>
        {
            new BackfillDispatchOrderStatusTravelBookingValidationTarget(new ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient(channel)),
            new BackfillDispatchOrderStatusAdmissionTarget(new ApiAdmissions.AdmissionService.AdmissionServiceClient(channel)),
            new BackfillDispatchOrderStatusCandidateCalculationTarget(new ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient(channel)),
            new BackfillDispatchOrderStatusProblemApprovalTarget(new ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient(channel)),
        };
        await sinks[0].PushAsync(record, token);
    }
}
