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
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

public static class OnboardingPlanQueryEndpointRequests
{
    /// <summary>
    /// Builds the CheckKnowledgeArticleEligibility request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest SummarizeOnboardingPlansToCheckKnowledgeArticleEligibility0(this ApiOnboarding.SummarizeOnboardingPlansRequest request)
    {
        return new ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Reason = (request.Reason ?? ""),
        };
    }
}
