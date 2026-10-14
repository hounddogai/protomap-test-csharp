using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>What SettleEnrollmentId keeps of each RejectEnrollment response.</summary>
internal sealed class SettleEnrollmentIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string EnrollmentId { get; init; } = "";
}

/// <summary>Sends records to acme.learning.v1.CourseEnrollmentCalculationService.</summary>
internal sealed class SettleEnrollmentIdCourseEnrollmentCalculationWriter(ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient client)
{
    public async Task WriteAsync(SettleEnrollmentIdRecord record, CancellationToken token)
    {
        await client.RecalculateCourseEnrollmentsAsync(new ApiLearning.RecalculateCourseEnrollmentsRequest { Reason = record.CorrelationId, CourseEnrollmentId = record.EnrollmentId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.benefits.v1.EnrollmentApprovalService/RejectEnrollment and hands what it keeps to acme.learning.v1.CourseEnrollmentCalculationService.</summary>
internal static class SettleEnrollmentId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient(channel);
        var reply = await source.RejectEnrollmentAsync(new ApiBenefits.RejectEnrollmentRequest() { Reason = "live-settle-enrollment-id" }, cancellationToken: token);
        var record = new SettleEnrollmentIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            EnrollmentId = reply.Enrollment.EnrollmentId,
        };
        await new SettleEnrollmentIdCourseEnrollmentCalculationWriter(new ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient(channel)).WriteAsync(record, token);
    }
}
