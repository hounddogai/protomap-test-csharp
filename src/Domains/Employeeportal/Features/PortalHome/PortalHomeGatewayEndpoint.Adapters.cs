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
using ApiDocuments = Acme.Documents.V1;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for ApproveVenue.
/// </summary>
public interface IPortalHomeGatewayEndpointSendComposePortalHomePage1Port
{
    Task<ApiCorporateevents.ApproveVenueResponse> SendAsync(ApiCorporateevents.ApproveVenueRequest request, CancellationToken token);
}

public sealed class PortalHomeGatewayEndpointSendComposePortalHomePage1PortAdapter(ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient client) : IPortalHomeGatewayEndpointSendComposePortalHomePage1Port
{
    public async Task<ApiCorporateevents.ApproveVenueResponse> SendAsync(ApiCorporateevents.ApproveVenueRequest request, CancellationToken token)
    {
        using var rpc = client.ApproveVenueAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
