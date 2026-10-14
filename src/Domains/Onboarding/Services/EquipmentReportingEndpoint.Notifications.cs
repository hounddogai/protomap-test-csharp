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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

public sealed class EquipmentReportingEndpointShareEquipmentReport1NotificationHandler(ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.onboarding.v1.EquipmentReportingService/ShareEquipmentReport/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiOnboarding.PreviewOnboardingTaskRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.PreviewOnboardingTaskAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
