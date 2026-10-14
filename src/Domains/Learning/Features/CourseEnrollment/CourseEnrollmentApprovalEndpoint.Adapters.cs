using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for EstimateInterview.
/// </summary>
public interface ICourseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port
{
    Task<ApiRecruiting.EstimateInterviewResponse> SendAsync(ApiRecruiting.EstimateInterviewRequest request, CancellationToken token);
}

public sealed class CourseEnrollmentApprovalEndpointSendRejectCourseEnrollment3PortAdapter(ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient client) : ICourseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port
{
    public async Task<ApiRecruiting.EstimateInterviewResponse> SendAsync(ApiRecruiting.EstimateInterviewRequest request, CancellationToken token)
    {
        using var rpc = client.EstimateInterviewAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CourseEnrollmentApprovalEndpointRequests
{
    /// <summary>
    /// Builds the CommentLearningPathApproval request from the current workflow state.
    /// </summary>
    public static ApiLearning.CommentLearningPathApprovalRequest RejectCourseEnrollmentToCommentLearningPathApproval2(this ApiLearning.RejectCourseEnrollmentRequest request)
    {
        return new ApiLearning.CommentLearningPathApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.CourseEnrollment?.Audit?.CreatedBy ?? ""),
                        },
                    } },
                } },
            },
        };
    }
}
