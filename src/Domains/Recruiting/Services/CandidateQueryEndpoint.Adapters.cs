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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public static class CandidateQueryEndpointRequests
{
    /// <summary>
    /// Builds the GetCandidateRules request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.GetCandidateRulesRequest SummarizeCandidatesToGetCandidateRules0(this ApiRecruiting.SummarizeCandidatesRequest request)
    {
        return new ApiRecruiting.GetCandidateRulesRequest
        {
            CandidateId = (request.CandidateId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
