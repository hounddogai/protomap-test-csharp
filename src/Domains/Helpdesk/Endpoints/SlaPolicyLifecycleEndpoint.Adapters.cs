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

/// <summary>
/// The application port for PreviewTicket.
/// </summary>
public interface ISlaPolicyLifecycleEndpointSendRenewSlaPolicy0Port
{
    Task<ApiHelpdesk.PreviewTicketResponse> SendAsync(ApiHelpdesk.PreviewTicketRequest request, CancellationToken token);
}

public sealed class SlaPolicyLifecycleEndpointSendRenewSlaPolicy0PortAdapter(ApiHelpdesk.TicketValidationService.TicketValidationServiceClient client) : ISlaPolicyLifecycleEndpointSendRenewSlaPolicy0Port
{
    public async Task<ApiHelpdesk.PreviewTicketResponse> SendAsync(ApiHelpdesk.PreviewTicketRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewTicketAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class SlaPolicyLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the ExplainDocumentTemplateCalculation request from the current workflow state.
    /// </summary>
    public static ApiDocuments.ExplainDocumentTemplateCalculationRequest ReinstateSlaPolicyToExplainDocumentTemplateCalculation1(this ApiHelpdesk.ReinstateSlaPolicyRequest request)
    {
        return new ApiDocuments.ExplainDocumentTemplateCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainDocumentTemplateCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
    }
}
