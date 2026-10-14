using Grpc.Net.Client;
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where CopyCourseId hands each record.</summary>
internal interface ICopyCourseIdWriter
{
    Task DeliverAsync(CopyCourseIdRecord record, CancellationToken token);
}

/// <summary>What CopyCourseId keeps of each ValidateCourse response.</summary>
internal sealed class CopyCourseIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.learning.v1.CertificationValidationService.</summary>
internal sealed class CopyCourseIdCertificationValidationWriter(ApiLearning.CertificationValidationService.CertificationValidationServiceClient client) : ICopyCourseIdWriter
{
    public async Task DeliverAsync(CopyCourseIdRecord record, CancellationToken token)
    {
        await client.ValidateCertificationAsync(new ApiLearning.ValidateCertificationRequest { Certification = new ApiLearning.Certification { Number = record.Status }, Reason = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.recruiting.v1.CandidateCalculationService.</summary>
internal sealed class CopyCourseIdCandidateCalculationRelay(ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient client) : ICopyCourseIdWriter
{
    public async Task DeliverAsync(CopyCourseIdRecord record, CancellationToken token)
    {
        await client.GetCandidateRatesAsync(new ApiRecruiting.GetCandidateRatesRequest { CandidateId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.authorization.v1.AccessPolicyService.</summary>
internal sealed class CopyCourseIdAccessPolicyRelay(ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient client) : ICopyCourseIdWriter
{
    public async Task DeliverAsync(CopyCourseIdRecord record, CancellationToken token)
    {
        await client.CloneAccessPolicyAsync(new ApiAuthorization.CloneAccessPolicyRequest { AccessPolicy = new ApiAuthorization.AccessPolicy { Name = record.Status }, RequestId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.benefits.v1.LifeEventApprovalService.</summary>
internal sealed class CopyCourseIdLifeEventApprovalSink(ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient client) : ICopyCourseIdWriter
{
    public async Task DeliverAsync(CopyCourseIdRecord record, CancellationToken token)
    {
        await client.DelegateLifeEventApprovalAsync(new ApiBenefits.DelegateLifeEventApprovalRequest { LifeEvent = new ApiBenefits.LifeEvent { Source = record.Status, Kind = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.CourseValidationService/ValidateCourse and hands what it keeps to the sink of its kind.</summary>
internal static class CopyCourseId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CourseValidationService.CourseValidationServiceClient(channel);
        var response = await source.ValidateCourseAsync(new ApiLearning.ValidateCourseRequest() { CourseId = "default-copy-course-id" }, cancellationToken: token);
        var record = new CopyCourseIdRecord
        {
            ReferenceId = response.Outcome.ReferenceId,
            Status = response.Outcome.Status,
        };
        var sinks = new Dictionary<string, ICopyCourseIdWriter>
        {
            ["certification-validation"] = new CopyCourseIdCertificationValidationWriter(new ApiLearning.CertificationValidationService.CertificationValidationServiceClient(channel)),
            ["candidate-calculation"] = new CopyCourseIdCandidateCalculationRelay(new ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient(channel)),
            ["access-policy"] = new CopyCourseIdAccessPolicyRelay(new ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient(channel)),
            ["life-event-approval"] = new CopyCourseIdLifeEventApprovalSink(new ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient(channel)),
        };
        await RouteCopyCourseIdAsync("certification-validation", record, sinks, token);
    }

    private static async Task RouteCopyCourseIdAsync(string kind, CopyCourseIdRecord record, IReadOnlyDictionary<string, ICopyCourseIdWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
