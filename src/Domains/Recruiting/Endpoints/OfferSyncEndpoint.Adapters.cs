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
using ApiConfiguration = Acme.Configuration.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSponsorships = Acme.Sponsorships.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for ExplainRateLimitCalculation.
/// </summary>
public interface IOfferSyncEndpointSendReconcileOffers0Port
{
    Task<ApiRatelimiting.ExplainRateLimitCalculationResponse> SendAsync(ApiRatelimiting.ExplainRateLimitCalculationRequest request, CancellationToken token);
}

public sealed class OfferSyncEndpointSendReconcileOffers0PortAdapter(ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient client) : IOfferSyncEndpointSendReconcileOffers0Port
{
    public async Task<ApiRatelimiting.ExplainRateLimitCalculationResponse> SendAsync(ApiRatelimiting.ExplainRateLimitCalculationRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainRateLimitCalculationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
