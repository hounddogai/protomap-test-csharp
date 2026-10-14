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

public sealed class CourseWorkflowEndpointStartCourseWorkflow2NotificationHandler(ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.learning.v1.CourseWorkflowService/StartCourseWorkflow/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.ExplainCandidateDecisionRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ExplainCandidateDecisionAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
