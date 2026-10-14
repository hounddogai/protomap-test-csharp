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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Travel;

public static class TravelPolicyApprovalEndpointRequests
{
    /// <summary>
    /// Builds the GetTravelPolicyLifecycle request from the current workflow state.
    /// </summary>
    public static ApiTravel.GetTravelPolicyLifecycleRequest DelegateTravelPolicyApprovalToGetTravelPolicyLifecycle1(this ApiTravel.DelegateTravelPolicyApprovalRequest request)
    {
        return new ApiTravel.GetTravelPolicyLifecycleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicyId = (request.TravelPolicy?.TravelPolicyId ?? ""),
        };
    }
}
