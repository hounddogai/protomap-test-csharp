using Grpc.Net.Client;
using ApiDispatch = Acme.Dispatch.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where MirrorPrescriptionId hands each record.</summary>
internal interface IMirrorPrescriptionIdTarget
{
    Task DeliverAsync(MirrorPrescriptionIdRecord record, CancellationToken token);
}

/// <summary>What MirrorPrescriptionId keeps of each CalculatePrescription response.</summary>
internal sealed class MirrorPrescriptionIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Hands records to acme.dispatch.v1.DispatchOrderService.</summary>
internal sealed class MirrorPrescriptionIdDispatchOrderSink : IMirrorPrescriptionIdTarget
{
    private readonly ApiDispatch.DispatchOrderService.DispatchOrderServiceClient _client;

    public MirrorPrescriptionIdDispatchOrderSink(ApiDispatch.DispatchOrderService.DispatchOrderServiceClient client) => _client = client;

    public async Task DeliverAsync(MirrorPrescriptionIdRecord record, CancellationToken token)
    {
        await _client.CloneDispatchOrderAsync(new ApiDispatch.CloneDispatchOrderRequest { DispatchOrder = new ApiDispatch.DispatchOrder { Notes = record.Number, Number = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.insurance.v1.PolicyService.</summary>
internal sealed class MirrorPrescriptionIdPolicyRelay : IMirrorPrescriptionIdTarget
{
    private readonly ApiInsurance.PolicyService.PolicyServiceClient _client;

    public MirrorPrescriptionIdPolicyRelay(ApiInsurance.PolicyService.PolicyServiceClient client) => _client = client;

    public async Task DeliverAsync(MirrorPrescriptionIdRecord record, CancellationToken token)
    {
        await _client.SearchPoliciesAsync(new ApiInsurance.SearchPoliciesRequest { Filter = new ApiInsurance.SearchPoliciesRequest.Types.Filter { Query = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.learning.v1.CourseService.</summary>
internal sealed class MirrorPrescriptionIdCourseTarget : IMirrorPrescriptionIdTarget
{
    private readonly ApiLearning.CourseService.CourseServiceClient _client;

    public MirrorPrescriptionIdCourseTarget(ApiLearning.CourseService.CourseServiceClient client) => _client = client;

    public async Task DeliverAsync(MirrorPrescriptionIdRecord record, CancellationToken token)
    {
        await _client.CloneCourseAsync(new ApiLearning.CloneCourseRequest { Course = new ApiLearning.Course { Name = record.CorrelationId, CourseId = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class MirrorPrescriptionIdLogOnly : IMirrorPrescriptionIdTarget
{
    public Task DeliverAsync(MirrorPrescriptionIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.prescriptions.v1.PrescriptionCalculationService/CalculatePrescription and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorPrescriptionId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient(channel);
        var response = await source.CalculatePrescriptionAsync(new ApiPrescriptions.CalculatePrescriptionRequest() { PrescriptionId = "nightly-mirror-prescription-id" }, cancellationToken: token);
        var record = new MirrorPrescriptionIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Number = response.Prescription.Number,
        };
        await RouteMirrorPrescriptionIdAsync("dispatch-order", record, channel, token);
    }

    private static async Task RouteMirrorPrescriptionIdAsync(string kind, MirrorPrescriptionIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IMirrorPrescriptionIdTarget sink = new MirrorPrescriptionIdLogOnly();
        switch (kind)
        {
            case "dispatch-order":
                sink = new MirrorPrescriptionIdDispatchOrderSink(new ApiDispatch.DispatchOrderService.DispatchOrderServiceClient(channel));
                break;
            case "policy":
                sink = new MirrorPrescriptionIdPolicyRelay(new ApiInsurance.PolicyService.PolicyServiceClient(channel));
                break;
            case "course":
                sink = new MirrorPrescriptionIdCourseTarget(new ApiLearning.CourseService.CourseServiceClient(channel));
                break;
        }
        await sink.DeliverAsync(record, token);
    }
}
