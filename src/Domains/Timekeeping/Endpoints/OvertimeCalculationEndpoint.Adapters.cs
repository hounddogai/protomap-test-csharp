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
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

public static class OvertimeCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ValidateTimesheet request from the current workflow state.
    /// </summary>
    public static ApiTimekeeping.ValidateTimesheetRequest ExplainOvertimeCalculationToValidateTimesheet0(this ApiTimekeeping.ExplainOvertimeCalculationRequest request)
    {
        return new ApiTimekeeping.ValidateTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ValidateTimesheetRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Overtime?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
        };
    }
}
