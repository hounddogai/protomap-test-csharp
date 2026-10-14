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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

public sealed class IncidentCalculationEndpointSimulateIncident2NotificationHandler(ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.itsm.v1.IncidentCalculationService/SimulateIncident/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.PreviewLicenseAssignmentRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.PreviewLicenseAssignmentAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
