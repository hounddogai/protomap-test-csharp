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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

public sealed class OnboardingPlanLifecycleEndpointActivateOnboardingPlan0NotificationHandler(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.onboarding.v1.OnboardingPlanLifecycleService/ActivateOnboardingPlan/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiOnboarding.CompareOnboardingTaskOptionsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CompareOnboardingTaskOptionsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
