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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for GenerateInterviewReport.
/// </summary>
public interface ICandidateLifecycleEndpointSendSuspendCandidate0Port
{
    Task<ApiRecruiting.GenerateInterviewReportResponse> SendAsync(ApiRecruiting.GenerateInterviewReportRequest request, CancellationToken token);
}

public sealed class CandidateLifecycleEndpointSendSuspendCandidate0PortAdapter(ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient client) : ICandidateLifecycleEndpointSendSuspendCandidate0Port
{
    public async Task<ApiRecruiting.GenerateInterviewReportResponse> SendAsync(ApiRecruiting.GenerateInterviewReportRequest request, CancellationToken token)
    {
        using var rpc = client.GenerateInterviewReportAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
