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
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for RetryDevicePolicyWorkflow.
/// </summary>
public interface IIncidentWorkflowEndpointSendPauseIncidentWorkflow0Port
{
    Task<ApiDevices.RetryDevicePolicyWorkflowResponse> SendAsync(ApiDevices.RetryDevicePolicyWorkflowRequest request, CancellationToken token);
}

public sealed class IncidentWorkflowEndpointSendPauseIncidentWorkflow0PortAdapter(ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient client) : IIncidentWorkflowEndpointSendPauseIncidentWorkflow0Port
{
    public async Task<ApiDevices.RetryDevicePolicyWorkflowResponse> SendAsync(ApiDevices.RetryDevicePolicyWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.RetryDevicePolicyWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class IncidentWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the SummarizeOrgUnits request from the current workflow state.
    /// </summary>
    public static ApiEmployees.SummarizeOrgUnitsRequest ListIncidentWorkflowsToSummarizeOrgUnits0(this ApiItsm.ListIncidentWorkflowsRequest request)
    {
        return new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
    }
}
