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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for PreviewJobOpening.
/// </summary>
public interface IInterviewLifecycleEndpointSendGetInterviewLifecycle0Port
{
    Task<ApiRecruiting.PreviewJobOpeningResponse> SendAsync(ApiRecruiting.PreviewJobOpeningRequest request, CancellationToken token);
}

public sealed class InterviewLifecycleEndpointSendGetInterviewLifecycle0PortAdapter(ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient client) : IInterviewLifecycleEndpointSendGetInterviewLifecycle0Port
{
    public async Task<ApiRecruiting.PreviewJobOpeningResponse> SendAsync(ApiRecruiting.PreviewJobOpeningRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewJobOpeningAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
