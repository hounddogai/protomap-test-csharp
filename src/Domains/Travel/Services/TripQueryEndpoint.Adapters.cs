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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for ListLedgerAccounts.
/// </summary>
public interface ITripQueryEndpointSendResolveTrips0Port
{
    Task<ApiLedger.ListLedgerAccountsResponse> SendAsync(ApiLedger.ListLedgerAccountsRequest request, CancellationToken token);
}

public sealed class TripQueryEndpointSendResolveTrips0PortAdapter(ApiLedger.LedgerAccountService.LedgerAccountServiceClient client) : ITripQueryEndpointSendResolveTrips0Port
{
    public async Task<ApiLedger.ListLedgerAccountsResponse> SendAsync(ApiLedger.ListLedgerAccountsRequest request, CancellationToken token)
    {
        using var rpc = client.ListLedgerAccountsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TripQueryEndpointRequests
{
    /// <summary>
    /// Builds the CheckTravelBookingEligibility request from the current workflow state.
    /// </summary>
    public static ApiTravel.CheckTravelBookingEligibilityRequest PeekTripToCheckTravelBookingEligibility0(this ApiTravel.PeekTripRequest request)
    {
        return new ApiTravel.CheckTravelBookingEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
    }
}
