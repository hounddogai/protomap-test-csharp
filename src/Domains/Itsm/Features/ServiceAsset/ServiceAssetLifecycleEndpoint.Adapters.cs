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
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for StartDevicePolicyWorkflow.
/// </summary>
public interface IServiceAssetLifecycleEndpointSendTerminateServiceAsset0Port
{
    Task<ApiDevices.StartDevicePolicyWorkflowResponse> SendAsync(ApiDevices.StartDevicePolicyWorkflowRequest request, CancellationToken token);
}

public sealed class ServiceAssetLifecycleEndpointSendTerminateServiceAsset0PortAdapter(ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient client) : IServiceAssetLifecycleEndpointSendTerminateServiceAsset0Port
{
    public async Task<ApiDevices.StartDevicePolicyWorkflowResponse> SendAsync(ApiDevices.StartDevicePolicyWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.StartDevicePolicyWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ServiceAssetLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the GenerateChangeRequestReport request from the current workflow state.
    /// </summary>
    public static ApiItsm.GenerateChangeRequestReportRequest SuspendServiceAssetToGenerateChangeRequestReport1(this ApiItsm.SuspendServiceAssetRequest request, ApiItsm.CheckServiceAssetEligibilityResponse reply0)
    {
        return new ApiItsm.GenerateChangeRequestReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.GenerateChangeRequestReportRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
    }
}
