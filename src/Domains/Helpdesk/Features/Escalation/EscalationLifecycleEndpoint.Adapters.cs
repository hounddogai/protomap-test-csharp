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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for StartSoftwareLicenseWorkflow.
/// </summary>
public interface IEscalationLifecycleEndpointSendTerminateEscalation0Port
{
    Task<ApiLicensing.StartSoftwareLicenseWorkflowResponse> SendAsync(ApiLicensing.StartSoftwareLicenseWorkflowRequest request, CancellationToken token);
}

public sealed class EscalationLifecycleEndpointSendTerminateEscalation0PortAdapter(ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient client) : IEscalationLifecycleEndpointSendTerminateEscalation0Port
{
    public async Task<ApiLicensing.StartSoftwareLicenseWorkflowResponse> SendAsync(ApiLicensing.StartSoftwareLicenseWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.StartSoftwareLicenseWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EscalationLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the QuoteSlaPolicy request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.QuoteSlaPolicyRequest SuspendEscalationToQuoteSlaPolicy1(this ApiHelpdesk.SuspendEscalationRequest request, ApiLicensing.EscalateSoftwareLicenseResponse reply0)
    {
        return new ApiHelpdesk.QuoteSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
        };
    }
}
