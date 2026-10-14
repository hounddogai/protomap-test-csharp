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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public static class TravelPolicyWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the BatchGetCarts request from the current workflow state.
    /// </summary>
    public static ApiCart.BatchGetCartsRequest ResumeTravelPolicyWorkflowToBatchGetCarts2(this ApiTravel.ResumeTravelPolicyWorkflowRequest request)
    {
        return new ApiCart.BatchGetCartsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
