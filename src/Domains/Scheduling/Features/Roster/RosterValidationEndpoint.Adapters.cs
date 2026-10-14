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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for ExplainRosterCalculation.
/// </summary>
public interface IRosterValidationEndpointSendPreviewRoster1Port
{
    Task<ApiScheduling.ExplainRosterCalculationResponse> SendAsync(ApiScheduling.ExplainRosterCalculationRequest request, CancellationToken token);
}

public sealed class RosterValidationEndpointSendPreviewRoster1PortAdapter(ApiScheduling.RosterCalculationService.RosterCalculationServiceClient client) : IRosterValidationEndpointSendPreviewRoster1Port
{
    public async Task<ApiScheduling.ExplainRosterCalculationResponse> SendAsync(ApiScheduling.ExplainRosterCalculationRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainRosterCalculationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
