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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for FindLedgerAccounts.
/// </summary>
public interface ITravelPolicyLifecycleEndpointSendActivateTravelPolicy3Port
{
    Task<ApiLedger.FindLedgerAccountsResponse> SendAsync(ApiLedger.FindLedgerAccountsRequest request, CancellationToken token);
}

public sealed class TravelPolicyLifecycleEndpointSendActivateTravelPolicy3PortAdapter(ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient client) : ITravelPolicyLifecycleEndpointSendActivateTravelPolicy3Port
{
    public async Task<ApiLedger.FindLedgerAccountsResponse> SendAsync(ApiLedger.FindLedgerAccountsRequest request, CancellationToken token)
    {
        using var rpc = client.FindLedgerAccountsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TravelPolicyLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the GetTravelPolicySyncState request from the current workflow state.
    /// </summary>
    public static ApiTravel.GetTravelPolicySyncStateRequest ActivateTravelPolicyToGetTravelPolicySyncState2(this ApiTravel.ActivateTravelPolicyRequest request)
    {
        return new ApiTravel.GetTravelPolicySyncStateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicyId = (request.TravelPolicyId ?? ""),
        };
    }
}
