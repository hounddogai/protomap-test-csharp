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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPatientapp = Acme.Patientapp.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for ListPortalHomeSubscriptions.
/// </summary>
public interface IAnnouncementNotificationEndpointSendSendAnnouncementTestNotice0Port
{
    Task<ApiEmployeeportal.ListPortalHomeSubscriptionsResponse> SendAsync(ApiEmployeeportal.ListPortalHomeSubscriptionsRequest request, CancellationToken token);
}

public sealed class AnnouncementNotificationEndpointSendSendAnnouncementTestNotice0PortAdapter(ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient client) : IAnnouncementNotificationEndpointSendSendAnnouncementTestNotice0Port
{
    public async Task<ApiEmployeeportal.ListPortalHomeSubscriptionsResponse> SendAsync(ApiEmployeeportal.ListPortalHomeSubscriptionsRequest request, CancellationToken token)
    {
        using var rpc = client.ListPortalHomeSubscriptionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
