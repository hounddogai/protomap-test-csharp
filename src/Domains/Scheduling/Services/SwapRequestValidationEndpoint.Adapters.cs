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
using ApiConfiguration = Acme.Configuration.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public static class SwapRequestValidationEndpointRequests
{
    /// <summary>
    /// Builds the ResolveLearningPaths request from the current workflow state.
    /// </summary>
    public static ApiLearning.ResolveLearningPathsRequest CheckSwapRequestEligibilityToResolveLearningPaths0(this ApiScheduling.CheckSwapRequestEligibilityRequest request)
    {
        return new ApiLearning.ResolveLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
