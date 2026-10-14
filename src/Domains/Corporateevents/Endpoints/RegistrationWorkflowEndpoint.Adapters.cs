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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for SignalTicketWorkflow.
/// </summary>
public interface IRegistrationWorkflowEndpointSendSignalRegistrationWorkflow0Port
{
    Task<ApiHelpdesk.SignalTicketWorkflowResponse> SendAsync(ApiHelpdesk.SignalTicketWorkflowRequest request, CancellationToken token);
}

public sealed class RegistrationWorkflowEndpointSendSignalRegistrationWorkflow0PortAdapter(ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient client) : IRegistrationWorkflowEndpointSendSignalRegistrationWorkflow0Port
{
    public async Task<ApiHelpdesk.SignalTicketWorkflowResponse> SendAsync(ApiHelpdesk.SignalTicketWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.SignalTicketWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RegistrationWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the AdvanceLicenseAssignmentWorkflow request from the current workflow state.
    /// </summary>
    public static ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest RetryRegistrationWorkflowToAdvanceLicenseAssignmentWorkflow1(this ApiCorporateevents.RetryRegistrationWorkflowRequest request, ApiCorporateevents.GetCorporateEventApprovalStatusResponse reply0)
    {
        return new ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
    }
}
