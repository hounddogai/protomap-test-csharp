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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public static class TravelPolicyValidationEndpointRequests
{
    /// <summary>
    /// Builds the EstimateTravelBooking request from the current workflow state.
    /// </summary>
    public static ApiTravel.EstimateTravelBookingRequest PreviewTravelPolicyToEstimateTravelBooking2(this ApiTravel.PreviewTravelPolicyRequest request, ApiTravel.CalculateItineraryResponse reply0)
    {
        return new ApiTravel.EstimateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.EstimateTravelBookingRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Itinerary?.BillingAddress?.Locality ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply0.Itinerary?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
    }
}
