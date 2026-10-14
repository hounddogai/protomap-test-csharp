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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for GetCourseEnrollmentRules.
/// </summary>
public interface ICourseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port
{
    Task<ApiLearning.GetCourseEnrollmentRulesResponse> SendAsync(ApiLearning.GetCourseEnrollmentRulesRequest request, CancellationToken token);
}

public sealed class CourseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1PortAdapter(ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient client) : ICourseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port
{
    public async Task<ApiLearning.GetCourseEnrollmentRulesResponse> SendAsync(ApiLearning.GetCourseEnrollmentRulesRequest request, CancellationToken token)
    {
        using var rpc = client.GetCourseEnrollmentRulesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CourseEnrollmentLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the SimulateCertification request from the current workflow state.
    /// </summary>
    public static ApiLearning.SimulateCertificationRequest ActivateCourseEnrollmentToSimulateCertification0(this ApiLearning.ActivateCourseEnrollmentRequest request)
    {
        return new ApiLearning.SimulateCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
    }
}
