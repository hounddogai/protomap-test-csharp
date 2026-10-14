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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for ExplainYardCalculation.
/// </summary>
public interface IOfferCalculationEndpointSendSimulateOffer0Port
{
    Task<ApiYards.ExplainYardCalculationResponse> SendAsync(ApiYards.ExplainYardCalculationRequest request, CancellationToken token);
}

public sealed class OfferCalculationEndpointSendSimulateOffer0PortAdapter(ApiYards.YardCalculationService.YardCalculationServiceClient client) : IOfferCalculationEndpointSendSimulateOffer0Port
{
    public async Task<ApiYards.ExplainYardCalculationResponse> SendAsync(ApiYards.ExplainYardCalculationRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainYardCalculationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
