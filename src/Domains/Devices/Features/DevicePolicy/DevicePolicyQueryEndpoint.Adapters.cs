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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSeating = Acme.Seating.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The application port for NormalizeVenue.
/// </summary>
public interface IDevicePolicyQueryEndpointSendExportDevicePolicies2Port
{
    Task<ApiVenues.NormalizeVenueResponse> SendAsync(ApiVenues.NormalizeVenueRequest request, CancellationToken token);
}

public sealed class DevicePolicyQueryEndpointSendExportDevicePolicies2PortAdapter(ApiVenues.VenueValidationService.VenueValidationServiceClient client) : IDevicePolicyQueryEndpointSendExportDevicePolicies2Port
{
    public async Task<ApiVenues.NormalizeVenueResponse> SendAsync(ApiVenues.NormalizeVenueRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizeVenueAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class DevicePolicyQueryEndpointRequests
{
    /// <summary>
    /// Builds the CheckDocumentEligibility request from the current workflow state.
    /// </summary>
    public static ApiDocuments.CheckDocumentEligibilityRequest ExportDevicePoliciesToCheckDocumentEligibility1(this ApiDevices.ExportDevicePoliciesRequest request)
    {
        return new ApiDocuments.CheckDocumentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CheckDocumentEligibilityRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
    }
}
