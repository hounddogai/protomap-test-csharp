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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRisk = Acme.Risk.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for ResumeTicketWorkflow.
/// </summary>
public interface IRegistrationReportingEndpointSendScheduleRegistrationReport3Port
{
    Task<ApiHelpdesk.ResumeTicketWorkflowResponse> SendAsync(ApiHelpdesk.ResumeTicketWorkflowRequest request, CancellationToken token);
}

public sealed class RegistrationReportingEndpointSendScheduleRegistrationReport3PortAdapter(ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient client) : IRegistrationReportingEndpointSendScheduleRegistrationReport3Port
{
    public async Task<ApiHelpdesk.ResumeTicketWorkflowResponse> SendAsync(ApiHelpdesk.ResumeTicketWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.ResumeTicketWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
