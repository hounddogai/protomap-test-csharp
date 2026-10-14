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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

public sealed class FeedbackWorkflowEndpointCancelFeedbackWorkflow3NotificationHandler(ApiIdentity.UserValidationService.UserValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.performance.v1.FeedbackWorkflowService/CancelFeedbackWorkflow/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiIdentity.ScoreUserRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ScoreUserAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
