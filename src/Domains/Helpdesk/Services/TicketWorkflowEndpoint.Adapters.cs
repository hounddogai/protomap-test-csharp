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
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for ResumeEntitlementWorkflow.
/// </summary>
public interface ITicketWorkflowEndpointSendAdvanceTicketWorkflow1Port
{
    Task<ApiLicensing.ResumeEntitlementWorkflowResponse> SendAsync(ApiLicensing.ResumeEntitlementWorkflowRequest request, CancellationToken token);
}

public sealed class TicketWorkflowEndpointSendAdvanceTicketWorkflow1PortAdapter(ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient client) : ITicketWorkflowEndpointSendAdvanceTicketWorkflow1Port
{
    public async Task<ApiLicensing.ResumeEntitlementWorkflowResponse> SendAsync(ApiLicensing.ResumeEntitlementWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.ResumeEntitlementWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TicketWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the ScheduleSlaPolicyExpiry request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.ScheduleSlaPolicyExpiryRequest AdvanceTicketWorkflowToScheduleSlaPolicyExpiry0(this ApiHelpdesk.AdvanceTicketWorkflowRequest request)
    {
        return new ApiHelpdesk.ScheduleSlaPolicyExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScheduleSlaPolicyExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Ticket?.Requester?.Phone ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    UserId = (request.Ticket?.Assignee?.UserId ?? ""),
                },
            },
        };
    }
}
