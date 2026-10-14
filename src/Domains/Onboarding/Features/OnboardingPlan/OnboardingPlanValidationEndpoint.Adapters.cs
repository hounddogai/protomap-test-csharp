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

/// <summary>
/// The application port for RecalculateOnboardingTasks.
/// </summary>
public interface IOnboardingPlanValidationEndpointSendValidateOnboardingPlan0Port
{
    Task<ApiOnboarding.RecalculateOnboardingTasksResponse> SendAsync(ApiOnboarding.RecalculateOnboardingTasksRequest request, CancellationToken token);
}

public sealed class OnboardingPlanValidationEndpointSendValidateOnboardingPlan0PortAdapter(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) : IOnboardingPlanValidationEndpointSendValidateOnboardingPlan0Port
{
    public async Task<ApiOnboarding.RecalculateOnboardingTasksResponse> SendAsync(ApiOnboarding.RecalculateOnboardingTasksRequest request, CancellationToken token)
    {
        using var rpc = client.RecalculateOnboardingTasksAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
