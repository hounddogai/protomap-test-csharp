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
using ApiDevices = Acme.Devices.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Devices;

public sealed class ComplianceScanApprovalEndpointApproveComplianceScan1NotificationHandler(ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient client) : INotificationHandler
{
    public string Topic => "acme.devices.v1.ComplianceScanApprovalService/ApproveComplianceScan/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDevices.AdvanceManagedDeviceWorkflowRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.AdvanceManagedDeviceWorkflowAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
