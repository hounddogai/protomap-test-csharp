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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

public static class ProblemValidationEndpointRequests
{
    /// <summary>
    /// Builds the ResolveSoftwareLicenses request from the current workflow state.
    /// </summary>
    public static ApiLicensing.ResolveSoftwareLicensesRequest CheckProblemEligibilityToResolveSoftwareLicenses1(this ApiItsm.CheckProblemEligibilityRequest request)
    {
        return new ApiLicensing.ResolveSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
