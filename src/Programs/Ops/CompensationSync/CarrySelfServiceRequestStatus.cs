using Grpc.Net.Client;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLearning = Acme.Learning.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where CarrySelfServiceRequestStatus hands each record.</summary>
internal interface ICarrySelfServiceRequestStatusSink
{
    Task WriteAsync(CarrySelfServiceRequestStatusRecord record, CancellationToken token);
}

/// <summary>What CarrySelfServiceRequestStatus keeps of each DeleteSelfServiceRequest response.</summary>
internal sealed class CarrySelfServiceRequestStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.learning.v1.CourseService.</summary>
internal sealed class CarrySelfServiceRequestStatusCourseRelay : ICarrySelfServiceRequestStatusSink
{
    private readonly ApiLearning.CourseService.CourseServiceClient _client;

    public CarrySelfServiceRequestStatusCourseRelay(ApiLearning.CourseService.CourseServiceClient client) => _client = client;

    public async Task WriteAsync(CarrySelfServiceRequestStatusRecord record, CancellationToken token)
    {
        await _client.UpdateCourseAsync(new ApiLearning.UpdateCourseRequest { Course = new ApiLearning.Course { Code = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.onboarding.v1.OnboardingTaskCalculationService.</summary>
internal sealed class CarrySelfServiceRequestStatusOnboardingTaskCalculationRelay(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) : ICarrySelfServiceRequestStatusSink
{
    public async Task WriteAsync(CarrySelfServiceRequestStatusRecord record, CancellationToken token)
    {
        await client.GetOnboardingTaskRatesAsync(new ApiOnboarding.GetOnboardingTaskRatesRequest { OnboardingTaskId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employeeportal.v1.SelfServiceRequestService/DeleteSelfServiceRequest and hands what it keeps to the sink of its kind.</summary>
internal static class CarrySelfServiceRequestStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployeeportal.SelfServiceRequestService.SelfServiceRequestServiceClient(channel);
        var got = await source.DeleteSelfServiceRequestAsync(new ApiEmployeeportal.DeleteSelfServiceRequestRequest() { SelfServiceRequestId = "live-carry-self-service-request-status" }, cancellationToken: token);
        var record = new CarrySelfServiceRequestStatusRecord
        {
            Status = got.Result.Status,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ICarrySelfServiceRequestStatusSink>
        {
            ["course"] = new CarrySelfServiceRequestStatusCourseRelay(new ApiLearning.CourseService.CourseServiceClient(channel)),
            ["onboarding-task-calculation"] = new CarrySelfServiceRequestStatusOnboardingTaskCalculationRelay(new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(channel)),
        };
        await RouteCarrySelfServiceRequestStatusAsync("onboarding-task-calculation", record, sinks, token);
    }

    private static async Task RouteCarrySelfServiceRequestStatusAsync(string kind, CarrySelfServiceRequestStatusRecord record, IReadOnlyDictionary<string, ICarrySelfServiceRequestStatusSink> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
