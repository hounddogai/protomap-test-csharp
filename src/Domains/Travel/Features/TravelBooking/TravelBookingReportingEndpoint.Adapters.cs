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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for ListTravelPolicyConflicts.
/// </summary>
public interface ITravelBookingReportingEndpointSendScheduleTravelBookingReport0Port
{
    Task<ApiTravel.ListTravelPolicyConflictsResponse> SendAsync(ApiTravel.ListTravelPolicyConflictsRequest request, CancellationToken token);
}

public sealed class TravelBookingReportingEndpointSendScheduleTravelBookingReport0PortAdapter(ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient client) : ITravelBookingReportingEndpointSendScheduleTravelBookingReport0Port
{
    public async Task<ApiTravel.ListTravelPolicyConflictsResponse> SendAsync(ApiTravel.ListTravelPolicyConflictsRequest request, CancellationToken token)
    {
        using var rpc = client.ListTravelPolicyConflictsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TravelBookingReportingEndpointRequests
{
    /// <summary>
    /// Builds the ResolveAuditEvents request from the current workflow state.
    /// </summary>
    public static ApiAudit.ResolveAuditEventsRequest GenerateTravelBookingReportToResolveAuditEvents1(this ApiTravel.GenerateTravelBookingReportRequest request)
    {
        return new ApiAudit.ResolveAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
