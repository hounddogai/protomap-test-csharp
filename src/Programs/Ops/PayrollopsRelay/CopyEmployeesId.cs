using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where CopyEmployeesId hands each record.</summary>
internal interface ICopyEmployeesIdSink
{
    Task ApplyAsync(CopyEmployeesIdRecord record, CancellationToken token);
}

/// <summary>What CopyEmployeesId keeps of each CountEmployees response.</summary>
internal sealed class CopyEmployeesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.learning.v1.CourseEnrollmentValidationService.</summary>
internal sealed class CopyEmployeesIdCourseEnrollmentValidationSink(ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient client) : ICopyEmployeesIdSink
{
    public async Task ApplyAsync(CopyEmployeesIdRecord record, CancellationToken token)
    {
        await client.ValidateCourseEnrollmentAsync(new ApiLearning.ValidateCourseEnrollmentRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.venues.v1.VenueQueryService.</summary>
internal sealed class CopyEmployeesIdVenueQueryTarget : ICopyEmployeesIdSink
{
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _client;

    public CopyEmployeesIdVenueQueryTarget(ApiVenues.VenueQueryService.VenueQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(CopyEmployeesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiVenues.SuggestVenuesRequest { OrderBy = record.CorrelationId, Filter = new ApiVenues.SuggestVenuesRequest.Types.Filter { Region = record.Message } }, token, _client.SuggestVenuesAsync);
    }
}

/// <summary>Sends records to acme.performance.v1.FeedbackCalculationService.</summary>
internal sealed class CopyEmployeesIdFeedbackCalculationRelay : ICopyEmployeesIdSink
{
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _client;

    public CopyEmployeesIdFeedbackCalculationRelay(ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(CopyEmployeesIdRecord record, CancellationToken token)
    {
        await _client.GetFeedbackRatesAsync(new ApiPerformance.GetFeedbackRatesRequest { FeedbackId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employees.v1.EmployeeQueryService/CountEmployees and hands what it keeps to the sink of its kind.</summary>
internal static class CopyEmployeesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient(channel);
        var reply = await source.CountEmployeesAsync(new ApiEmployees.CountEmployeesRequest() { OrderBy = "current-copy-employees-id" }, cancellationToken: token);
        var record = new CopyEmployeesIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
            Message = reply.Result.Message,
        };
        await ActOnCopyEmployeesIdAsync("venue-query", record, channel, token);
    }

    private static async Task ActOnCopyEmployeesIdAsync(string kind, CopyEmployeesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["course-enrollment-validation"] = () => new CopyEmployeesIdCourseEnrollmentValidationSink(new ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient(channel)).ApplyAsync(record, token),
            ["venue-query"] = () => new CopyEmployeesIdVenueQueryTarget(new ApiVenues.VenueQueryService.VenueQueryServiceClient(channel)).ApplyAsync(record, token),
            ["feedback-calculation"] = () => new CopyEmployeesIdFeedbackCalculationRelay(new ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
