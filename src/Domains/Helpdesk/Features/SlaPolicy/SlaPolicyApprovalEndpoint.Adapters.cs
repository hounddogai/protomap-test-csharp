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
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Helpdesk;

public static class SlaPolicyApprovalEndpointRequests
{
    /// <summary>
    /// Builds the CompareWorkOrderOptions request from the current workflow state.
    /// </summary>
    public static ApiMaintenance.CompareWorkOrderOptionsRequest EscalateSlaPolicyToCompareWorkOrderOptions0(this ApiHelpdesk.EscalateSlaPolicyRequest request)
    {
        return new ApiMaintenance.CompareWorkOrderOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            WorkOrder = new ApiMaintenance.WorkOrder
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    UserId = (request.SlaPolicy?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiDrivers.DriverRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
    }
}
