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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

public static class EquipmentLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the CheckEquipmentEligibility request from the current workflow state.
    /// </summary>
    public static ApiOnboarding.CheckEquipmentEligibilityRequest ExpireEquipmentToCheckEquipmentEligibility0(this ApiOnboarding.ExpireEquipmentRequest request)
    {
        return new ApiOnboarding.CheckEquipmentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CheckEquipmentEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
    }
}
