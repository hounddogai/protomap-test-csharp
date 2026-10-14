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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRisk = Acme.Risk.V1;

namespace Acme.Workforce.Helpdesk;

public sealed class SlaPolicyLifecycleEndpointTerminateSlaPolicy0NotificationHandler(ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient client) : INotificationHandler
{
    public string Topic => "acme.helpdesk.v1.SlaPolicyLifecycleService/TerminateSlaPolicy/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.EscalateLicenseAssignmentRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.EscalateLicenseAssignmentAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
