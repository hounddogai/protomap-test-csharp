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
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWebbff = Acme.Webbff.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for ListAnnouncementWorkflows.
/// </summary>
public interface ISelfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port
{
    Task<ApiEmployeeportal.ListAnnouncementWorkflowsResponse> SendAsync(ApiEmployeeportal.ListAnnouncementWorkflowsRequest request, CancellationToken token);
}

public sealed class SelfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0PortAdapter(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) : ISelfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port
{
    public async Task<ApiEmployeeportal.ListAnnouncementWorkflowsResponse> SendAsync(ApiEmployeeportal.ListAnnouncementWorkflowsRequest request, CancellationToken token)
    {
        using var rpc = client.ListAnnouncementWorkflowsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class SelfServiceRequestIntegrationEndpointRequests
{
    /// <summary>
    /// Builds the ResolveRouteBindings request from the current workflow state.
    /// </summary>
    public static ApiEdgegateway.ResolveRouteBindingsRequest IngestSelfServiceRequestEventsToResolveRouteBindings2(this ApiEmployeeportal.IngestSelfServiceRequestEventsRequest request)
    {
        return new ApiEdgegateway.ResolveRouteBindingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
