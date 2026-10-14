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
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for LintJobOpenings.
/// </summary>
public interface IInterviewApprovalEndpointSendEscalateInterview0Port
{
    Task<ApiRecruiting.LintJobOpeningsResponse> SendAsync(ApiRecruiting.LintJobOpeningsRequest request, CancellationToken token);
}

public sealed class InterviewApprovalEndpointSendEscalateInterview0PortAdapter(ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient client) : IInterviewApprovalEndpointSendEscalateInterview0Port
{
    public async Task<ApiRecruiting.LintJobOpeningsResponse> SendAsync(ApiRecruiting.LintJobOpeningsRequest request, CancellationToken token)
    {
        using var rpc = client.LintJobOpeningsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
