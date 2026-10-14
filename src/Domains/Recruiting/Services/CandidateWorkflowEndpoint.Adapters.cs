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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for AdvanceJobOpeningWorkflow.
/// </summary>
public interface ICandidateWorkflowEndpointSendListCandidateWorkflows0Port
{
    Task<ApiRecruiting.AdvanceJobOpeningWorkflowResponse> SendAsync(ApiRecruiting.AdvanceJobOpeningWorkflowRequest request, CancellationToken token);
}

public sealed class CandidateWorkflowEndpointSendListCandidateWorkflows0PortAdapter(ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient client) : ICandidateWorkflowEndpointSendListCandidateWorkflows0Port
{
    public async Task<ApiRecruiting.AdvanceJobOpeningWorkflowResponse> SendAsync(ApiRecruiting.AdvanceJobOpeningWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.AdvanceJobOpeningWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
