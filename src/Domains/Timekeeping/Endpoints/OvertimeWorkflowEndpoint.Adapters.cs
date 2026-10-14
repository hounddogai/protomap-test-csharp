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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTours = Acme.Tours.V1;

namespace Acme.Workforce.Timekeeping;

public static class OvertimeWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the DelegateLearningPathApproval request from the current workflow state.
    /// </summary>
    public static ApiLearning.DelegateLearningPathApprovalRequest InspectOvertimeWorkflowToDelegateLearningPathApproval1(this ApiTimekeeping.InspectOvertimeWorkflowRequest request, ApiLearning.EscalateLearningPathResponse reply0)
    {
        return new ApiLearning.DelegateLearningPathApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply0.LearningPath?.Location?.CountryCode ?? ""),
                        },
                    } },
                } },
            },
        };
    }
}
