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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for ResumeOnboardingPlanWorkflow.
/// </summary>
public interface IEquipmentApprovalEndpointSendApproveEquipment0Port
{
    Task<ApiOnboarding.ResumeOnboardingPlanWorkflowResponse> SendAsync(ApiOnboarding.ResumeOnboardingPlanWorkflowRequest request, CancellationToken token);
}

public sealed class EquipmentApprovalEndpointSendApproveEquipment0PortAdapter(ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient client) : IEquipmentApprovalEndpointSendApproveEquipment0Port
{
    public async Task<ApiOnboarding.ResumeOnboardingPlanWorkflowResponse> SendAsync(ApiOnboarding.ResumeOnboardingPlanWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.ResumeOnboardingPlanWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EquipmentApprovalEndpointRequests
{
    /// <summary>
    /// Builds the RetryOnboardingPlanWorkflow request from the current workflow state.
    /// </summary>
    public static ApiOnboarding.RetryOnboardingPlanWorkflowRequest SubmitEquipmentToRetryOnboardingPlanWorkflow1(this ApiOnboarding.SubmitEquipmentRequest request)
    {
        return new ApiOnboarding.RetryOnboardingPlanWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Equipment?.Audit?.CreatedBy ?? ""),
                },
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Description = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                    Quantity = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
    }
}
