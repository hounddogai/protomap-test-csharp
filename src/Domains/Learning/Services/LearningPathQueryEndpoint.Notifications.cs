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
using ApiDispatch = Acme.Dispatch.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public sealed class LearningPathQueryEndpointResolveLearningPaths1NotificationHandler(ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.learning.v1.LearningPathQueryService/ResolveLearningPaths/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLearning.SimulateCertificationRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SimulateCertificationAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
