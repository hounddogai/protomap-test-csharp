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
using ApiAssets = Acme.Assets.V1;
using ApiCommon = Acme.Common.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for PreviewPosition.
/// </summary>
public interface IItineraryApprovalEndpointSendRejectItinerary0Port
{
    Task<ApiEmployees.PreviewPositionResponse> SendAsync(ApiEmployees.PreviewPositionRequest request, CancellationToken token);
}

public sealed class ItineraryApprovalEndpointSendRejectItinerary0PortAdapter(ApiEmployees.PositionValidationService.PositionValidationServiceClient client) : IItineraryApprovalEndpointSendRejectItinerary0Port
{
    public async Task<ApiEmployees.PreviewPositionResponse> SendAsync(ApiEmployees.PreviewPositionRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewPositionAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
