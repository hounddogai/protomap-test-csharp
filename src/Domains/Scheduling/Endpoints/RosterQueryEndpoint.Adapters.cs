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
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for ExplainSwapRequestDecision.
/// </summary>
public interface IRosterQueryEndpointSendSuggestRosters1Port
{
    Task<ApiScheduling.ExplainSwapRequestDecisionResponse> SendAsync(ApiScheduling.ExplainSwapRequestDecisionRequest request, CancellationToken token);
}

public sealed class RosterQueryEndpointSendSuggestRosters1PortAdapter(ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient client) : IRosterQueryEndpointSendSuggestRosters1Port
{
    public async Task<ApiScheduling.ExplainSwapRequestDecisionResponse> SendAsync(ApiScheduling.ExplainSwapRequestDecisionRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainSwapRequestDecisionAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
