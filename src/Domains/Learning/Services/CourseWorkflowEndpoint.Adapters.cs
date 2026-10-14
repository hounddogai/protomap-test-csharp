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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for ValidateCandidate.
/// </summary>
public interface ICourseWorkflowEndpointSendStartCourseWorkflow1Port
{
    Task<ApiRecruiting.ValidateCandidateResponse> SendAsync(ApiRecruiting.ValidateCandidateRequest request, CancellationToken token);
}

public sealed class CourseWorkflowEndpointSendStartCourseWorkflow1PortAdapter(ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient client) : ICourseWorkflowEndpointSendStartCourseWorkflow1Port
{
    public async Task<ApiRecruiting.ValidateCandidateResponse> SendAsync(ApiRecruiting.ValidateCandidateRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateCandidateAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
