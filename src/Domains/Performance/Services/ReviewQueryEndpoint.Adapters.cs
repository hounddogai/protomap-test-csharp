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
using ApiAnalytics = Acme.Analytics.V1;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public static class ReviewQueryEndpointRequests
{
    /// <summary>
    /// Builds the CompareCalibrationOptions request from the current workflow state.
    /// </summary>
    public static ApiPerformance.CompareCalibrationOptionsRequest SummarizeReviewsToCompareCalibrationOptions0(this ApiPerformance.SummarizeReviewsRequest request)
    {
        return new ApiPerformance.CompareCalibrationOptionsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
