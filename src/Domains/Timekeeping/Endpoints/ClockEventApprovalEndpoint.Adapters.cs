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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPatients = Acme.Patients.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The application port for CommentIncidentApproval.
/// </summary>
public interface IClockEventApprovalEndpointSendCommentClockEventApproval0Port
{
    Task<ApiItsm.CommentIncidentApprovalResponse> SendAsync(ApiItsm.CommentIncidentApprovalRequest request, CancellationToken token);
}

public sealed class ClockEventApprovalEndpointSendCommentClockEventApproval0PortAdapter(ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient client) : IClockEventApprovalEndpointSendCommentClockEventApproval0Port
{
    public async Task<ApiItsm.CommentIncidentApprovalResponse> SendAsync(ApiItsm.CommentIncidentApprovalRequest request, CancellationToken token)
    {
        using var rpc = client.CommentIncidentApprovalAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ClockEventApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ExplainPositionDecision request from the current workflow state.
    /// </summary>
    public static ApiEmployees.ExplainPositionDecisionRequest ApproveClockEventToExplainPositionDecision0(this ApiTimekeeping.ApproveClockEventRequest request)
    {
        return new ApiEmployees.ExplainPositionDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
    }
    /// <summary>
    /// Builds the CancelSlaPolicyWorkflow request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.CancelSlaPolicyWorkflowRequest RejectClockEventToCancelSlaPolicyWorkflow1(this ApiTimekeeping.RejectClockEventRequest request, ApiEmployees.ExplainEmploymentContractCalculationResponse reply0)
    {
        return new ApiHelpdesk.CancelSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
    }
}
