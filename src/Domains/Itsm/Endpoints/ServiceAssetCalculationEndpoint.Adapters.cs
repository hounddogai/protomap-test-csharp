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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

public static class ServiceAssetCalculationEndpointRequests
{
    /// <summary>
    /// Builds the EstimateChangeRequest request from the current workflow state.
    /// </summary>
    public static ApiItsm.EstimateChangeRequestRequest CalculateServiceAssetToEstimateChangeRequest1(this ApiItsm.CalculateServiceAssetRequest request, ApiLicensing.CalculateSoftwareLicenseResponse reply0)
    {
        return new ApiItsm.EstimateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
    /// <summary>
    /// Builds the SummarizeAccessPolicies request from the current workflow state.
    /// </summary>
    public static ApiAuthorization.SummarizeAccessPoliciesRequest RecalculateServiceAssetsToSummarizeAccessPolicies3(this ApiItsm.RecalculateServiceAssetsRequest request)
    {
        return new ApiAuthorization.SummarizeAccessPoliciesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
    }
}
