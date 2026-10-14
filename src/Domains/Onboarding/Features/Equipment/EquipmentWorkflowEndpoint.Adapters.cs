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
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for RejectOnboardingPlan.
/// </summary>
public interface IEquipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port
{
    Task<ApiOnboarding.RejectOnboardingPlanResponse> SendAsync(ApiOnboarding.RejectOnboardingPlanRequest request, CancellationToken token);
}

public sealed class EquipmentWorkflowEndpointSendCancelEquipmentWorkflow0PortAdapter(ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient client) : IEquipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port
{
    public async Task<ApiOnboarding.RejectOnboardingPlanResponse> SendAsync(ApiOnboarding.RejectOnboardingPlanRequest request, CancellationToken token)
    {
        using var rpc = client.RejectOnboardingPlanAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EquipmentWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the GetEquipmentLifecycle request from the current workflow state.
    /// </summary>
    public static ApiOnboarding.GetEquipmentLifecycleRequest AdvanceEquipmentWorkflowToGetEquipmentLifecycle2(this ApiOnboarding.AdvanceEquipmentWorkflowRequest request)
    {
        return new ApiOnboarding.GetEquipmentLifecycleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EquipmentId = (request.Equipment?.EquipmentId ?? ""),
        };
    }
}
