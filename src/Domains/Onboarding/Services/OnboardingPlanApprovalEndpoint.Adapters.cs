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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for BrowseOnboardingPlans.
/// </summary>
public interface IOnboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port
{
    Task<ApiOnboarding.BrowseOnboardingPlansResponse> SendAsync(ApiOnboarding.BrowseOnboardingPlansRequest request, CancellationToken token);
}

public sealed class OnboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0PortAdapter(ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient client) : IOnboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port
{
    public async Task<ApiOnboarding.BrowseOnboardingPlansResponse> SendAsync(ApiOnboarding.BrowseOnboardingPlansRequest request, CancellationToken token)
    {
        using var rpc = client.BrowseOnboardingPlansAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OnboardingPlanApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ResolveOnboardingPlans request from the current workflow state.
    /// </summary>
    public static ApiOnboarding.ResolveOnboardingPlansRequest EscalateOnboardingPlanToResolveOnboardingPlans1(this ApiOnboarding.EscalateOnboardingPlanRequest request)
    {
        return new ApiOnboarding.ResolveOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
