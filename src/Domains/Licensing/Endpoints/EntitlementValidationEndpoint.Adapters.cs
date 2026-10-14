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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMedia = Acme.Media.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for CheckVenueEligibility.
/// </summary>
public interface IEntitlementValidationEndpointSendExplainEntitlementDecision3Port
{
    Task<ApiVenues.CheckVenueEligibilityResponse> SendAsync(ApiVenues.CheckVenueEligibilityRequest request, CancellationToken token);
}

public sealed class EntitlementValidationEndpointSendExplainEntitlementDecision3PortAdapter(ApiVenues.VenueValidationService.VenueValidationServiceClient client) : IEntitlementValidationEndpointSendExplainEntitlementDecision3Port
{
    public async Task<ApiVenues.CheckVenueEligibilityResponse> SendAsync(ApiVenues.CheckVenueEligibilityRequest request, CancellationToken token)
    {
        using var rpc = client.CheckVenueEligibilityAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EntitlementValidationEndpointRequests
{
    /// <summary>
    /// Builds the SimulateSoftwareLicense request from the current workflow state.
    /// </summary>
    public static ApiLicensing.SimulateSoftwareLicenseRequest ExplainEntitlementDecisionToSimulateSoftwareLicense2(this ApiLicensing.ExplainEntitlementDecisionRequest request, ApiIdentity.CreateUserResponse reply1)
    {
        return new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.User?.Contact?.Email ?? ""),
                    Name = (reply1.User?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
    }
}
