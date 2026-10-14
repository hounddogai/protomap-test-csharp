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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for PauseAnnouncementWorkflow.
/// </summary>
public interface IAnnouncementIntegrationEndpointSendDisconnectAnnouncementSource3Port
{
    Task<ApiEmployeeportal.PauseAnnouncementWorkflowResponse> SendAsync(ApiEmployeeportal.PauseAnnouncementWorkflowRequest request, CancellationToken token);
}

public sealed class AnnouncementIntegrationEndpointSendDisconnectAnnouncementSource3PortAdapter(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) : IAnnouncementIntegrationEndpointSendDisconnectAnnouncementSource3Port
{
    public async Task<ApiEmployeeportal.PauseAnnouncementWorkflowResponse> SendAsync(ApiEmployeeportal.PauseAnnouncementWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.PauseAnnouncementWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class AnnouncementIntegrationEndpointRequests
{
    /// <summary>
    /// Builds the BrowseAccessPolicies request from the current workflow state.
    /// </summary>
    public static ApiAuthorization.BrowseAccessPoliciesRequest DisconnectAnnouncementSourceToBrowseAccessPolicies2(this ApiEmployeeportal.DisconnectAnnouncementSourceRequest request, ApiEmployeeportal.ListSelfServiceRequestSourcesResponse reply0, ApiEmployeeportal.ListAnnouncementWorkflowsResponse reply1)
    {
        return new ApiAuthorization.BrowseAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.BrowseAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.ReceivedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
    }
}
