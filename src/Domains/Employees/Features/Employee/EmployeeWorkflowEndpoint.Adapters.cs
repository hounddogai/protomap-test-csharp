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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiProcurement = Acme.Procurement.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The application port for RecallManagedDevice.
/// </summary>
public interface IEmployeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port
{
    Task<ApiDevices.RecallManagedDeviceResponse> SendAsync(ApiDevices.RecallManagedDeviceRequest request, CancellationToken token);
}

public sealed class EmployeeWorkflowEndpointSendAdvanceEmployeeWorkflow0PortAdapter(ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient client) : IEmployeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port
{
    public async Task<ApiDevices.RecallManagedDeviceResponse> SendAsync(ApiDevices.RecallManagedDeviceRequest request, CancellationToken token)
    {
        using var rpc = client.RecallManagedDeviceAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
