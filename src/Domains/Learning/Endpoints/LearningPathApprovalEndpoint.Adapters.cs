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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for ApproveCourse.
/// </summary>
public interface ILearningPathApprovalEndpointSendRecallLearningPath1Port
{
    Task<ApiLearning.ApproveCourseResponse> SendAsync(ApiLearning.ApproveCourseRequest request, CancellationToken token);
}

public sealed class LearningPathApprovalEndpointSendRecallLearningPath1PortAdapter(ApiLearning.CourseApprovalService.CourseApprovalServiceClient client) : ILearningPathApprovalEndpointSendRecallLearningPath1Port
{
    public async Task<ApiLearning.ApproveCourseResponse> SendAsync(ApiLearning.ApproveCourseRequest request, CancellationToken token)
    {
        using var rpc = client.ApproveCourseAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class LearningPathApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ListSignatureVersions request from the current workflow state.
    /// </summary>
    public static ApiDocuments.ListSignatureVersionsRequest RecallLearningPathToListSignatureVersions0(this ApiLearning.RecallLearningPathRequest request)
    {
        return new ApiDocuments.ListSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureVersionsRequest.Types.Filter
            {
                CreatedAfter = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.LearningPath?.Location?.Region ?? ""),
            },
        };
    }
}
