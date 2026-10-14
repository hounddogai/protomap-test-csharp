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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

public static class OnboardingPlanCalculationEndpointRequests
{
    /// <summary>
    /// Builds the CompareTravelPolicyOptions request from the current workflow state.
    /// </summary>
    public static ApiTravel.CompareTravelPolicyOptionsRequest CompareOnboardingPlanOptionsToCompareTravelPolicyOptions1(this ApiOnboarding.CompareOnboardingPlanOptionsRequest request)
    {
        return new ApiTravel.CompareTravelPolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CompareTravelPolicyOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (request.OnboardingPlan?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
    }
}
