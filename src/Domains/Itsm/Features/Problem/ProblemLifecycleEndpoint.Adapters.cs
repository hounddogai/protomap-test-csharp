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
using ApiConfiguration = Acme.Configuration.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for SummarizeOrgUnits.
/// </summary>
public interface IProblemLifecycleEndpointSendExpireProblem2Port
{
    Task<ApiEmployees.SummarizeOrgUnitsResponse> SendAsync(ApiEmployees.SummarizeOrgUnitsRequest request, CancellationToken token);
}

public sealed class ProblemLifecycleEndpointSendExpireProblem2PortAdapter(ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient client) : IProblemLifecycleEndpointSendExpireProblem2Port
{
    public async Task<ApiEmployees.SummarizeOrgUnitsResponse> SendAsync(ApiEmployees.SummarizeOrgUnitsRequest request, CancellationToken token)
    {
        using var rpc = client.SummarizeOrgUnitsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

/// <summary>
/// The application port for ValidateUser.
/// </summary>
public interface IProblemLifecycleEndpointSendTerminateProblem1Port
{
    Task<ApiIdentity.ValidateUserResponse> SendAsync(ApiIdentity.ValidateUserRequest request, CancellationToken token);
}

public sealed class ProblemLifecycleEndpointSendTerminateProblem1PortAdapter(ApiIdentity.UserValidationService.UserValidationServiceClient client) : IProblemLifecycleEndpointSendTerminateProblem1Port
{
    public async Task<ApiIdentity.ValidateUserResponse> SendAsync(ApiIdentity.ValidateUserRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateUserAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ProblemLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the CancelDevicePolicyWorkflow request from the current workflow state.
    /// </summary>
    public static ApiDevices.CancelDevicePolicyWorkflowRequest TerminateProblemToCancelDevicePolicyWorkflow0(this ApiItsm.TerminateProblemRequest request)
    {
        return new ApiDevices.CancelDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CancelDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
    }
}
