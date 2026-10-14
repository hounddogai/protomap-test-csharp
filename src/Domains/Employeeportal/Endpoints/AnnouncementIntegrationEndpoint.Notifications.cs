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

public sealed class AnnouncementIntegrationEndpointListAnnouncementSources0NotificationHandler(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) : INotificationHandler
{
    public string Topic => "acme.employeeportal.v1.AnnouncementIntegrationService/ListAnnouncementSources/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiEmployeeportal.RetryAnnouncementWorkflowRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.RetryAnnouncementWorkflowAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
