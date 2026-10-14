using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where CarryVisitorPassId hands each record.</summary>
internal interface ICarryVisitorPassIdDestination
{
    Task ApplyAsync(CarryVisitorPassIdRecord record, CancellationToken token);
}

/// <summary>What CarryVisitorPassId keeps of each ExpireVisitorPass response.</summary>
internal sealed class CarryVisitorPassIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.licensing.v1.EntitlementQueryService.</summary>
internal sealed class CarryVisitorPassIdEntitlementQueryWriter(ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient client) : ICarryVisitorPassIdDestination
{
    public async Task ApplyAsync(CarryVisitorPassIdRecord record, CancellationToken token)
    {
        await client.BrowseEntitlementsAsync(new ApiLicensing.BrowseEntitlementsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Message }, OrderBy = record.Notes, Filter = new ApiLicensing.BrowseEntitlementsRequest.Types.Filter { Query = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.recruiting.v1.CandidateApprovalService.</summary>
internal sealed class CarryVisitorPassIdCandidateApprovalRelay : ICarryVisitorPassIdDestination
{
    private readonly ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient _client;

    public CarryVisitorPassIdCandidateApprovalRelay(ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(CarryVisitorPassIdRecord record, CancellationToken token)
    {
        await _client.SubmitCandidateAsync(new ApiRecruiting.SubmitCandidateRequest { Details = new ApiRecruiting.SubmitCandidateRequest.Types.Details { ApproverEmail = record.Status, Comment = record.Message }, CandidateId = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.learning.v1.CourseEnrollmentLifecycleService.</summary>
internal sealed class CarryVisitorPassIdCourseEnrollmentLifecycleSink : ICarryVisitorPassIdDestination
{
    private readonly ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient _client;

    public CarryVisitorPassIdCourseEnrollmentLifecycleSink(ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(CarryVisitorPassIdRecord record, CancellationToken token)
    {
        await _client.TerminateCourseEnrollmentAsync(new ApiLearning.TerminateCourseEnrollmentRequest { Reason = record.Message, CourseEnrollmentId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.VisitorPassLifecycleService/ExpireVisitorPass and hands what it keeps to the sink of its kind.</summary>
internal static class CarryVisitorPassId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient(channel);
        var found = await source.ExpireVisitorPassAsync(new ApiWorkplaces.ExpireVisitorPassRequest() { VisitorPassId = "main-carry-visitor-pass-id" }, cancellationToken: token);
        var record = new CarryVisitorPassIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            Message = Text.Normalize(found.Result.Message),
            Notes = found.VisitorPass.Notes,
            Status = Text.Normalize(found.Result.Status),
        };
        var sinks = new Dictionary<string, ICarryVisitorPassIdDestination>
        {
            ["entitlement-query"] = new CarryVisitorPassIdEntitlementQueryWriter(new ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient(channel)),
            ["candidate-approval"] = new CarryVisitorPassIdCandidateApprovalRelay(new ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient(channel)),
            ["course-enrollment-lifecycle"] = new CarryVisitorPassIdCourseEnrollmentLifecycleSink(new ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient(channel)),
        };
        await RouteCarryVisitorPassIdAsync("candidate-approval", record, sinks, token);
    }

    private static async Task RouteCarryVisitorPassIdAsync(string kind, CarryVisitorPassIdRecord record, IReadOnlyDictionary<string, ICarryVisitorPassIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
