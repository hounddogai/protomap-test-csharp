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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTreasury = Acme.Treasury.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

public static class ShiftApprovalEndpointRequests
{
    /// <summary>
    /// Builds the LintEmploymentContracts request from the current workflow state.
    /// </summary>
    public static ApiEmployees.LintEmploymentContractsRequest EscalateShiftToLintEmploymentContracts2(this ApiScheduling.EscalateShiftRequest request, ApiVenues.LintVenuesResponse reply0, ApiWorkplaces.ShareVisitorPassReportResponse reply1)
    {
        return new ApiEmployees.LintEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintEmploymentContractsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Venue?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.VisitorPass?.BillingAddress?.CountryCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
    }
}
