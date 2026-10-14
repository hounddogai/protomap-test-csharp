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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public static class InterviewWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the StartRoomBookingWorkflow request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.StartRoomBookingWorkflowRequest PauseInterviewWorkflowToStartRoomBookingWorkflow1(this ApiRecruiting.PauseInterviewWorkflowRequest request)
    {
        return new ApiWorkplaces.StartRoomBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
    }
}
