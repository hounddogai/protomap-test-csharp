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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public static class ShiftQueryEndpointRequests
{
    /// <summary>
    /// Builds the SimulateSwapRequest request from the current workflow state.
    /// </summary>
    public static ApiScheduling.SimulateSwapRequestRequest ExportShiftsToSimulateSwapRequest1(this ApiScheduling.ExportShiftsRequest request, ApiAudit.ValidateAuditEventResponse reply0)
    {
        return new ApiScheduling.SimulateSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
    }
}
