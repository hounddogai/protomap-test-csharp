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
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public static class TripLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the ValidatePosition request from the current workflow state.
    /// </summary>
    public static ApiEmployees.ValidatePositionRequest ReinstateTripToValidatePosition1(this ApiTravel.ReinstateTripRequest request, ApiLicensing.ApproveEntitlementResponse reply0)
    {
        return new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidatePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Entitlement?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
    }
}
