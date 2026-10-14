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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public static class RoomBookingWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the PauseSlaPolicyWorkflow request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.PauseSlaPolicyWorkflowRequest StartRoomBookingWorkflowToPauseSlaPolicyWorkflow1(this ApiWorkplaces.StartRoomBookingWorkflowRequest request)
    {
        return new ApiHelpdesk.PauseSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.RoomBooking?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.RoomBooking?.Organizer?.Email ?? ""),
                    UserId = (request.RoomBooking?.Organizer?.UserId ?? ""),
                },
            },
        };
    }
}
