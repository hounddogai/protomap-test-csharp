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
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for CancelCourseWorkflow.
/// </summary>
public interface ILearningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port
{
    Task<ApiLearning.CancelCourseWorkflowResponse> SendAsync(ApiLearning.CancelCourseWorkflowRequest request, CancellationToken token);
}

public sealed class LearningPathWorkflowEndpointSendCancelLearningPathWorkflow0PortAdapter(ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient client) : ILearningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port
{
    public async Task<ApiLearning.CancelCourseWorkflowResponse> SendAsync(ApiLearning.CancelCourseWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.CancelCourseWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class LearningPathWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the DelegateCertificationApproval request from the current workflow state.
    /// </summary>
    public static ApiLearning.DelegateCertificationApprovalRequest AdvanceLearningPathWorkflowToDelegateCertificationApproval1(this ApiLearning.AdvanceLearningPathWorkflowRequest request, ApiLearning.ExplainCourseDecisionResponse reply0)
    {
        return new ApiLearning.DelegateCertificationApprovalRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Course?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.Course?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.LearningPath?.Location?.Region ?? ""),
                },
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Course?.Owner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
