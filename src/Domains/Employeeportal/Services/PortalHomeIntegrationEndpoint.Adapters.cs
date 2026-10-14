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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDatahub = Acme.Datahub.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for SuggestWorkspaces.
/// </summary>
public interface IPortalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port
{
    Task<ApiDatahub.SuggestWorkspacesResponse> SendAsync(ApiDatahub.SuggestWorkspacesRequest request, CancellationToken token);
}

public sealed class PortalHomeIntegrationEndpointSendReplayPortalHomeEvents0PortAdapter(ApiDatahub.WorkspaceQueryService.WorkspaceQueryServiceClient client) : IPortalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port
{
    public async Task<ApiDatahub.SuggestWorkspacesResponse> SendAsync(ApiDatahub.SuggestWorkspacesRequest request, CancellationToken token)
    {
        using var rpc = client.SuggestWorkspacesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
