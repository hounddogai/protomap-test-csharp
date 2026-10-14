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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Employeeportal;

public static class PortalHomeQueryEndpointRequests
{
    /// <summary>
    /// Builds the BrowseAuditEvents request from the current workflow state.
    /// </summary>
    public static ApiAudit.BrowseAuditEventsRequest SummarizePortalHomesToBrowseAuditEvents2(this ApiEmployeeportal.SummarizePortalHomesRequest request, ApiEmployeeportal.StartAnnouncementWorkflowResponse reply0, ApiEmployeeportal.AcknowledgePortalHomeEventResponse reply1)
    {
        return new ApiAudit.BrowseAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.BrowseAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (reply1.PortalHome?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.PortalHome?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Announcement?.Counterparty?.PortalHomeId ?? ""),
                Region = (reply0.Announcement?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
