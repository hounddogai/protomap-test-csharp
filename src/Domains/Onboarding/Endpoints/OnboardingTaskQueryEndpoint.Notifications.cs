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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMedia = Acme.Media.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

public sealed class OnboardingTaskQueryEndpointSummarizeOnboardingTasks1NotificationHandler(ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.onboarding.v1.OnboardingTaskQueryService/SummarizeOnboardingTasks/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiOnboarding.SummarizeOnboardingPlansRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SummarizeOnboardingPlansAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
