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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

public static class CertificationCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ExplainCourseDecision request from the current workflow state.
    /// </summary>
    public static ApiLearning.ExplainCourseDecisionRequest QuoteCertificationToExplainCourseDecision0(this ApiLearning.QuoteCertificationRequest request)
    {
        return new ApiLearning.ExplainCourseDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Certification?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
    }
}
