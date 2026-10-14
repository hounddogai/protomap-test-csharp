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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

public static class EmployeeApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ListVisitorPassConflicts request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.ListVisitorPassConflictsRequest CommentEmployeeApprovalToListVisitorPassConflicts1(this ApiEmployees.CommentEmployeeApprovalRequest request, ApiEmployees.CompareEmploymentContractOptionsResponse reply0)
    {
        return new ApiWorkplaces.ListVisitorPassConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.ListVisitorPassConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.Employee?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
