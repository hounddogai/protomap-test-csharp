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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Helpdesk;

public static class KnowledgeArticleQueryEndpointRequests
{
    /// <summary>
    /// Builds the CheckEscalationEligibility request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.CheckEscalationEligibilityRequest BrowseKnowledgeArticlesToCheckEscalationEligibility0(this ApiHelpdesk.BrowseKnowledgeArticlesRequest request)
    {
        return new ApiHelpdesk.CheckEscalationEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CheckEscalationEligibilityRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Sla = new ApiHelpdesk.EscalationSla
                {
                    DueAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
            },
        };
    }
}
