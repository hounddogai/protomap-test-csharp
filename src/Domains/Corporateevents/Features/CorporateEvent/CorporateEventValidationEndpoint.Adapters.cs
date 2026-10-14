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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for CountDispatchOrders.
/// </summary>
public interface ICorporateEventValidationEndpointSendGetCorporateEventRules1Port
{
    Task<ApiDispatch.CountDispatchOrdersResponse> SendAsync(ApiDispatch.CountDispatchOrdersRequest request, CancellationToken token);
}

public sealed class CorporateEventValidationEndpointSendGetCorporateEventRules1PortAdapter(ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient client) : ICorporateEventValidationEndpointSendGetCorporateEventRules1Port
{
    public async Task<ApiDispatch.CountDispatchOrdersResponse> SendAsync(ApiDispatch.CountDispatchOrdersRequest request, CancellationToken token)
    {
        using var rpc = client.CountDispatchOrdersAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CorporateEventValidationEndpointRequests
{
    /// <summary>
    /// Builds the SimulateCorporateEvent request from the current workflow state.
    /// </summary>
    public static ApiCorporateevents.SimulateCorporateEventRequest GetCorporateEventRulesToSimulateCorporateEvent0(this ApiCorporateevents.GetCorporateEventRulesRequest request)
    {
        return new ApiCorporateevents.SimulateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
            CorporateEventId = (request.CorporateEventId ?? ""),
        };
    }
}
