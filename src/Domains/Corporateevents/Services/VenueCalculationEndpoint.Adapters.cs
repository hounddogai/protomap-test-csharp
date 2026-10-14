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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for BatchGetBroadcasts.
/// </summary>
public interface IVenueCalculationEndpointSendQuoteVenue2Port
{
    Task<ApiBroadcasts.BatchGetBroadcastsResponse> SendAsync(ApiBroadcasts.BatchGetBroadcastsRequest request, CancellationToken token);
}

public sealed class VenueCalculationEndpointSendQuoteVenue2PortAdapter(ApiBroadcasts.BroadcastService.BroadcastServiceClient client) : IVenueCalculationEndpointSendQuoteVenue2Port
{
    public async Task<ApiBroadcasts.BatchGetBroadcastsResponse> SendAsync(ApiBroadcasts.BatchGetBroadcastsRequest request, CancellationToken token)
    {
        using var rpc = client.BatchGetBroadcastsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class VenueCalculationEndpointRequests
{
    /// <summary>
    /// Builds the CheckCorporateEventEligibility request from the current workflow state.
    /// </summary>
    public static ApiCorporateevents.CheckCorporateEventEligibilityRequest QuoteVenueToCheckCorporateEventEligibility1(this ApiCorporateevents.QuoteVenueRequest request, ApiCorporateevents.SimulateCorporateEventResponse reply0)
    {
        return new ApiCorporateevents.CheckCorporateEventEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Venue?.Owner?.Email ?? ""),
                    },
                } },
            },
        };
    }
}
