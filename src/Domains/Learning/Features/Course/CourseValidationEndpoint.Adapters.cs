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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for RecalculateLearningPaths.
/// </summary>
public interface ICourseValidationEndpointSendPreviewCourse2Port
{
    Task<ApiLearning.RecalculateLearningPathsResponse> SendAsync(ApiLearning.RecalculateLearningPathsRequest request, CancellationToken token);
}

public sealed class CourseValidationEndpointSendPreviewCourse2PortAdapter(ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient client) : ICourseValidationEndpointSendPreviewCourse2Port
{
    public async Task<ApiLearning.RecalculateLearningPathsResponse> SendAsync(ApiLearning.RecalculateLearningPathsRequest request, CancellationToken token)
    {
        using var rpc = client.RecalculateLearningPathsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CourseValidationEndpointRequests
{
    /// <summary>
    /// Builds the PeekUser request from the current workflow state.
    /// </summary>
    public static ApiIdentity.PeekUserRequest PreviewCourseToPeekUser1(this ApiLearning.PreviewCourseRequest request)
    {
        return new ApiIdentity.PeekUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Course?.Owner?.UserId ?? ""),
        };
    }
}
