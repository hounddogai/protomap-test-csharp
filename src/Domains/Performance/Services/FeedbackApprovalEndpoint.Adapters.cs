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
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public static class FeedbackApprovalEndpointRequests
{
    /// <summary>
    /// Builds the PreviewCandidate request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.PreviewCandidateRequest DelegateFeedbackApprovalToPreviewCandidate0(this ApiPerformance.DelegateFeedbackApprovalRequest request)
    {
        return new ApiRecruiting.PreviewCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
