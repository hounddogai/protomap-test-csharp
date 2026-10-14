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
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOrders = Acme.Orders.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

public static class ReviewWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the PreviewVenue request from the current workflow state.
    /// </summary>
    public static ApiVenues.PreviewVenueRequest StartReviewWorkflowToPreviewVenue0(this ApiPerformance.StartReviewWorkflowRequest request)
    {
        return new ApiVenues.PreviewVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Review?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
    }
}
