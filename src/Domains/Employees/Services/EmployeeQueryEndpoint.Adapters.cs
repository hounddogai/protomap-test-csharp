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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Employees;

public static class EmployeeQueryEndpointRequests
{
    /// <summary>
    /// Builds the BatchGetNotifications request from the current workflow state.
    /// </summary>
    public static ApiNotifications.BatchGetNotificationsRequest SummarizeEmployeesToBatchGetNotifications1(this ApiEmployees.SummarizeEmployeesRequest request)
    {
        return new ApiNotifications.BatchGetNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
