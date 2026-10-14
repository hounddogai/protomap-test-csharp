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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWebbff = Acme.Webbff.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for RetryAnnouncementWorkflow.
/// </summary>
public interface ISelfServiceRequestEndpointSendListSelfServiceRequests0Port
{
    Task<ApiEmployeeportal.RetryAnnouncementWorkflowResponse> SendAsync(ApiEmployeeportal.RetryAnnouncementWorkflowRequest request, CancellationToken token);
}

public sealed class SelfServiceRequestEndpointSendListSelfServiceRequests0PortAdapter(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) : ISelfServiceRequestEndpointSendListSelfServiceRequests0Port
{
    public async Task<ApiEmployeeportal.RetryAnnouncementWorkflowResponse> SendAsync(ApiEmployeeportal.RetryAnnouncementWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.RetryAnnouncementWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
