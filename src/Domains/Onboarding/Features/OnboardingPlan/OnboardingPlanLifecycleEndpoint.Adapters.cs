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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for EscalateEnrollment.
/// </summary>
public interface IOnboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port
{
    Task<ApiBenefits.EscalateEnrollmentResponse> SendAsync(ApiBenefits.EscalateEnrollmentRequest request, CancellationToken token);
}

public sealed class OnboardingPlanLifecycleEndpointSendRenewOnboardingPlan0PortAdapter(ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient client) : IOnboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port
{
    public async Task<ApiBenefits.EscalateEnrollmentResponse> SendAsync(ApiBenefits.EscalateEnrollmentRequest request, CancellationToken token)
    {
        using var rpc = client.EscalateEnrollmentAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OnboardingPlanLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the SummarizeOnboardingPlans request from the current workflow state.
    /// </summary>
    public static ApiOnboarding.SummarizeOnboardingPlansRequest ReinstateOnboardingPlanToSummarizeOnboardingPlans0(this ApiOnboarding.ReinstateOnboardingPlanRequest request)
    {
        return new ApiOnboarding.SummarizeOnboardingPlansRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SummarizeOnboardingPlansRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            Reason = (request.Reason ?? ""),
        };
    }
}
