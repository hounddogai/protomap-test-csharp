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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

public static class EscalationApprovalEndpointRequests
{
    /// <summary>
    /// Builds the LintKnowledgeArticles request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.LintKnowledgeArticlesRequest ApproveEscalationToLintKnowledgeArticles0(this ApiHelpdesk.ApproveEscalationRequest request)
    {
        return new ApiHelpdesk.LintKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.LintKnowledgeArticlesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Escalation?.Assignee?.Email ?? ""),
                },
            },
        };
    }
    /// <summary>
    /// Builds the RejectTicket request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.RejectTicketRequest SubmitEscalationToRejectTicket1(this ApiHelpdesk.SubmitEscalationRequest request)
    {
        return new ApiHelpdesk.RejectTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
    }
}
