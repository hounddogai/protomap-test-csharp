using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPatientapp = Acme.Patientapp.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where SyncCourseStatus hands each record.</summary>
internal interface ISyncCourseStatusTarget
{
    Task SendAsync(SyncCourseStatusRecord record, CancellationToken token);
}

/// <summary>What SyncCourseStatus keeps of each SubmitCourse response.</summary>
internal sealed class SyncCourseStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string Code { get; init; } = "";
}

/// <summary>Passes records to acme.corporateevents.v1.RegistrationQueryService.</summary>
internal sealed class SyncCourseStatusRegistrationQueryTarget(ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient client) : ISyncCourseStatusTarget
{
    public async Task SendAsync(SyncCourseStatusRecord record, CancellationToken token)
    {
        await client.BrowseRegistrationsAsync(new ApiCorporateevents.BrowseRegistrationsRequest { Filter = new ApiCorporateevents.BrowseRegistrationsRequest.Types.Filter { Region = record.Code } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.itsm.v1.ChangeRequestWorkflowService.</summary>
internal sealed class SyncCourseStatusChangeRequestWorkflowTarget(ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient client) : ISyncCourseStatusTarget
{
    public async Task SendAsync(SyncCourseStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.StartChangeRequestWorkflowRequest { Reason = record.Status }, token, client.StartChangeRequestWorkflowAsync);
    }
}

/// <summary>Sends records to acme.yards.v1.YardQueryService.</summary>
internal sealed class SyncCourseStatusYardQueryTarget(ApiYards.YardQueryService.YardQueryServiceClient client) : ISyncCourseStatusTarget
{
    public async Task SendAsync(SyncCourseStatusRecord record, CancellationToken token)
    {
        await client.SummarizeYardsAsync(new ApiYards.SummarizeYardsRequest { Details = new ApiYards.SummarizeYardsRequest.Types.Details { Cron = record.Code }, YardId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.patientapp.v1.PatientHomeQueryService.</summary>
internal sealed class SyncCourseStatusPatientHomeQueryWriter : ISyncCourseStatusTarget
{
    private readonly ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient _client;

    public SyncCourseStatusPatientHomeQueryWriter(ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient client) => _client = client;

    public async Task SendAsync(SyncCourseStatusRecord record, CancellationToken token)
    {
        await _client.CountPatientHomesAsync(new ApiPatientapp.CountPatientHomesRequest { Filter = new ApiPatientapp.CountPatientHomesRequest.Types.Filter { Query = record.Message }, Page = new ApiCommon.PageRequest { PageToken = record.Code } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.CourseApprovalService/SubmitCourse and hands what it keeps to the sink of its kind.</summary>
internal static class SyncCourseStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CourseApprovalService.CourseApprovalServiceClient(channel);
        var response = await source.SubmitCourseAsync(new ApiLearning.SubmitCourseRequest() { Reason = "live-sync-course-status" }, cancellationToken: token);
        var record = new SyncCourseStatusRecord
        {
            Status = response.Outcome.Status,
            Message = Text.Normalize(response.Outcome.Message),
            Code = Text.Normalize(response.Course.Code),
        };
        var sinks = new Dictionary<string, ISyncCourseStatusTarget>
        {
            ["registration-query"] = new SyncCourseStatusRegistrationQueryTarget(new ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient(channel)),
            ["change-request-workflow"] = new SyncCourseStatusChangeRequestWorkflowTarget(new ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient(channel)),
            ["yard-query"] = new SyncCourseStatusYardQueryTarget(new ApiYards.YardQueryService.YardQueryServiceClient(channel)),
            ["patient-home-query"] = new SyncCourseStatusPatientHomeQueryWriter(new ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("SYNC_COURSE_STATUS_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].SendAsync(record, token);
    }
}
