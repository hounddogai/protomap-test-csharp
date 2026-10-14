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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTours = Acme.Tours.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for ValidateTravelBooking.
/// </summary>
public interface IItineraryCalculationEndpointSendGetItineraryRates0Port
{
    Task<ApiTravel.ValidateTravelBookingResponse> SendAsync(ApiTravel.ValidateTravelBookingRequest request, CancellationToken token);
}

public sealed class ItineraryCalculationEndpointSendGetItineraryRates0PortAdapter(ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient client) : IItineraryCalculationEndpointSendGetItineraryRates0Port
{
    public async Task<ApiTravel.ValidateTravelBookingResponse> SendAsync(ApiTravel.ValidateTravelBookingRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateTravelBookingAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ItineraryCalculationEndpointRequests
{
    /// <summary>
    /// Builds the CountAuditEvents request from the current workflow state.
    /// </summary>
    public static ApiAudit.CountAuditEventsRequest ExplainItineraryCalculationToCountAuditEvents1(this ApiTravel.ExplainItineraryCalculationRequest request, ApiLedger.SuggestLedgerAccountsResponse reply0)
    {
        return new ApiAudit.CountAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.CountAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Itinerary?.Counterparty?.TravelBookingId ?? ""),
                Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
