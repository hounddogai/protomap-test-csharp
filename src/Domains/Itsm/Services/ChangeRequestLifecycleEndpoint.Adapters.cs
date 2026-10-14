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
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for ValidateNotification.
/// </summary>
public interface IChangeRequestLifecycleEndpointSendGetChangeRequestLifecycle0Port
{
    Task<ApiNotifications.ValidateNotificationResponse> SendAsync(ApiNotifications.ValidateNotificationRequest request, CancellationToken token);
}

public sealed class ChangeRequestLifecycleEndpointSendGetChangeRequestLifecycle0PortAdapter(ApiNotifications.NotificationValidationService.NotificationValidationServiceClient client) : IChangeRequestLifecycleEndpointSendGetChangeRequestLifecycle0Port
{
    public async Task<ApiNotifications.ValidateNotificationResponse> SendAsync(ApiNotifications.ValidateNotificationRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateNotificationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ChangeRequestLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the PreviewSearchIndex request from the current workflow state.
    /// </summary>
    public static ApiSearch.PreviewSearchIndexRequest ExpireChangeRequestToPreviewSearchIndex1(this ApiItsm.ExpireChangeRequestRequest request)
    {
        return new ApiSearch.PreviewSearchIndexRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
    }
}
