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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

public static class VenueSyncEndpointRequests
{
    /// <summary>
    /// Builds the RetryDevicePolicyWorkflow request from the current workflow state.
    /// </summary>
    public static ApiDevices.RetryDevicePolicyWorkflowRequest StreamVenueChangesToRetryDevicePolicyWorkflow2(this ApiCorporateevents.StreamVenueChangesRequest request, ApiDocuments.CheckDocumentEligibilityResponse reply0, ApiCorporateevents.CompareRegistrationOptionsResponse reply1)
    {
        return new ApiDevices.RetryDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Registration?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Document?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
    }
}
