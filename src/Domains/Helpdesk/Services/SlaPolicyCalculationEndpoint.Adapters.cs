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
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSeating = Acme.Seating.V1;

namespace Acme.Workforce.Helpdesk;

public static class SlaPolicyCalculationEndpointRequests
{
    /// <summary>
    /// Builds the CalculateServiceAsset request from the current workflow state.
    /// </summary>
    public static ApiItsm.CalculateServiceAssetRequest CompareSlaPolicyOptionsToCalculateServiceAsset1(this ApiHelpdesk.CompareSlaPolicyOptionsRequest request, ApiHelpdesk.CheckTicketEligibilityResponse reply0)
    {
        return new ApiItsm.CalculateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SlaPolicy?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.Ticket?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Ticket?.Requester?.Email ?? ""),
                },
            },
        };
    }
}
