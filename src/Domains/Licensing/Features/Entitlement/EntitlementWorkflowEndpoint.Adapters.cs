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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Licensing;

public static class EntitlementWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the QuoteTrip request from the current workflow state.
    /// </summary>
    public static ApiTravel.QuoteTripRequest ResumeEntitlementWorkflowToQuoteTrip0(this ApiLicensing.ResumeEntitlementWorkflowRequest request)
    {
        return new ApiTravel.QuoteTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Entitlement?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Entitlement?.Owner?.DisplayName ?? ""),
                    Email = (request.Entitlement?.Owner?.Email ?? ""),
                },
            },
        };
    }
}
