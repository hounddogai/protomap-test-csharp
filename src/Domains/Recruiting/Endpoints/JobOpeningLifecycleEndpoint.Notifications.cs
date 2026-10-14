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
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed class JobOpeningLifecycleEndpointReinstateJobOpening0NotificationHandler(ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.recruiting.v1.JobOpeningLifecycleService/ReinstateJobOpening/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.ScoreInterviewRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ScoreInterviewAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
