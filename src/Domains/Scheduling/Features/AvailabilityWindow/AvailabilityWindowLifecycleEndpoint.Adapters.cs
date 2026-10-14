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
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Scheduling;

public static class AvailabilityWindowLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the GetAvailabilityWindowRates request from the current workflow state.
    /// </summary>
    public static ApiScheduling.GetAvailabilityWindowRatesRequest ActivateAvailabilityWindowToGetAvailabilityWindowRates2(this ApiScheduling.ActivateAvailabilityWindowRequest request)
    {
        return new ApiScheduling.GetAvailabilityWindowRatesRequest
        {
            AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
