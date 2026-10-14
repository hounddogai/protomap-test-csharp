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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The application port for EscalateEnrollment.
/// </summary>
public interface IEnrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port
{
    Task<ApiBenefits.EscalateEnrollmentResponse> SendAsync(ApiBenefits.EscalateEnrollmentRequest request, CancellationToken token);
}

public sealed class EnrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0PortAdapter(ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient client) : IEnrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port
{
    public async Task<ApiBenefits.EscalateEnrollmentResponse> SendAsync(ApiBenefits.EscalateEnrollmentRequest request, CancellationToken token)
    {
        using var rpc = client.EscalateEnrollmentAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EnrollmentWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the NormalizeOffer request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.NormalizeOfferRequest RetryEnrollmentWorkflowToNormalizeOffer2(this ApiBenefits.RetryEnrollmentWorkflowRequest request)
    {
        return new ApiRecruiting.NormalizeOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.NormalizeOfferRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
    }
}
