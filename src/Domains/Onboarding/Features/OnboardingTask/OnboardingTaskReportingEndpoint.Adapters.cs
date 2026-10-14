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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiReviews = Acme.Reviews.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for SignalBuildingWorkflow.
/// </summary>
public interface IOnboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port
{
    Task<ApiWorkplaces.SignalBuildingWorkflowResponse> SendAsync(ApiWorkplaces.SignalBuildingWorkflowRequest request, CancellationToken token);
}

public sealed class OnboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2PortAdapter(ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient client) : IOnboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port
{
    public async Task<ApiWorkplaces.SignalBuildingWorkflowResponse> SendAsync(ApiWorkplaces.SignalBuildingWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.SignalBuildingWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OnboardingTaskReportingEndpointRequests
{
    /// <summary>
    /// Builds the AdvanceLearningPathWorkflow request from the current workflow state.
    /// </summary>
    public static ApiLearning.AdvanceLearningPathWorkflowRequest GenerateOnboardingTaskReportToAdvanceLearningPathWorkflow1(this ApiOnboarding.GenerateOnboardingTaskReportRequest request, ApiLearning.RetryLearningPathWorkflowResponse reply0)
    {
        return new ApiLearning.AdvanceLearningPathWorkflowRequest
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
                            CreatedAt = (reply0.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                    } },
                } },
            },
        };
    }
}
