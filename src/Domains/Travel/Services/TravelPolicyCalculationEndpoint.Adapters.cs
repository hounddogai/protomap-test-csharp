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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public static class TravelPolicyCalculationEndpointRequests
{
    /// <summary>
    /// Builds the PreviewProblem request from the current workflow state.
    /// </summary>
    public static ApiItsm.PreviewProblemRequest CompareTravelPolicyOptionsToPreviewProblem0(this ApiTravel.CompareTravelPolicyOptionsRequest request)
    {
        return new ApiItsm.PreviewProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.TravelPolicy?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
    }
}
