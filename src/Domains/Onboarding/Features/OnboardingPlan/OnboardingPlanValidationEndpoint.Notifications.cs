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
using ApiAudiences = Acme.Audiences.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Onboarding;

public sealed class OnboardingPlanValidationEndpointExplainOnboardingPlanDecision1NotificationHandler(ApiDrivers.DriverService.DriverServiceClient client) : INotificationHandler
{
    public string Topic => "acme.onboarding.v1.OnboardingPlanValidationService/ExplainOnboardingPlanDecision/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDrivers.CloneDriverRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CloneDriverAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
