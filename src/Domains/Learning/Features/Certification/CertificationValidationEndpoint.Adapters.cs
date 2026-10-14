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
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCommon = Acme.Common.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public static class CertificationValidationEndpointRequests
{
    /// <summary>
    /// Builds the SearchIncidents request from the current workflow state.
    /// </summary>
    public static ApiItsm.SearchIncidentsRequest PreviewCertificationToSearchIncidents1(this ApiLearning.PreviewCertificationRequest request)
    {
        return new ApiItsm.SearchIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.SearchIncidentsRequest.Types.Filter
            {
                Region = (request.Certification?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
