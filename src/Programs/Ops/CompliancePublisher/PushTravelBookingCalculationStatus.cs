using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiLearning = Acme.Learning.V1;
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where PushTravelBookingCalculationStatus hands each record.</summary>
internal interface IPushTravelBookingCalculationStatusWriter
{
    Task ForwardAsync(PushTravelBookingCalculationStatusRecord record, CancellationToken token);
}

/// <summary>What PushTravelBookingCalculationStatus keeps of each ExplainTravelBookingCalculation response.</summary>
internal sealed class PushTravelBookingCalculationStatusRecord
{
    public string Status { get; init; } = "";
    public string Number { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Passes records to acme.learning.v1.CertificationWorkflowService.</summary>
internal sealed class PushTravelBookingCalculationStatusCertificationWorkflowRelay(ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient client) : IPushTravelBookingCalculationStatusWriter
{
    public async Task ForwardAsync(PushTravelBookingCalculationStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.RetryCertificationWorkflowRequest { Details = new ApiLearning.RetryCertificationWorkflowRequest.Types.Details { Step = record.Number, InitiatedBy = record.Notes }, Certification = new ApiLearning.Certification { Number = record.Status } }, token, client.RetryCertificationWorkflowAsync);
    }
}

/// <summary>Writes records to acme.modelregistry.v1.RegisteredModelService.</summary>
internal sealed class PushTravelBookingCalculationStatusRegisteredModelWriter : IPushTravelBookingCalculationStatusWriter
{
    private readonly ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient _client;

    public PushTravelBookingCalculationStatusRegisteredModelWriter(ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient client) => _client = client;

    public async Task ForwardAsync(PushTravelBookingCalculationStatusRecord record, CancellationToken token)
    {
        await _client.CreateRegisteredModelAsync(new ApiModelregistry.CreateRegisteredModelRequest { RequestId = record.Number, RegisteredModel = new ApiModelregistry.RegisteredModel { RegisteredModelId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushTravelBookingCalculationStatusLogOnly : IPushTravelBookingCalculationStatusWriter
{
    public Task ForwardAsync(PushTravelBookingCalculationStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.travel.v1.TravelBookingCalculationService/ExplainTravelBookingCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class PushTravelBookingCalculationStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient(channel);
        var reply = await source.ExplainTravelBookingCalculationAsync(new ApiTravel.ExplainTravelBookingCalculationRequest() { TravelBookingId = "main-push-travel-booking-calculation-status" }, cancellationToken: token);
        var record = new PushTravelBookingCalculationStatusRecord
        {
            Status = reply.Outcome.Status,
            Number = Text.Normalize(reply.TravelBooking.Number),
            Notes = reply.TravelBooking.Notes,
        };
        await DeliverPushTravelBookingCalculationStatusAsync("certification-workflow", record, channel, token);
    }

    private static async Task DeliverPushTravelBookingCalculationStatusAsync(string kind, PushTravelBookingCalculationStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPushTravelBookingCalculationStatusWriter sink = kind switch
        {
            "certification-workflow" => new PushTravelBookingCalculationStatusCertificationWorkflowRelay(new ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient(channel)),
            "registered-model" => new PushTravelBookingCalculationStatusRegisteredModelWriter(new ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient(channel)),
            _ => new PushTravelBookingCalculationStatusLogOnly(),
        };
        await sink.ForwardAsync(record, token);
    }
}
