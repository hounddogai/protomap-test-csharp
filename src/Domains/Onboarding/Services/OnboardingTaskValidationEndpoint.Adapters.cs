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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for FindBenefitPlans.
/// </summary>
public interface IOnboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port
{
    Task<ApiBenefits.FindBenefitPlansResponse> SendAsync(ApiBenefits.FindBenefitPlansRequest request, CancellationToken token);
}

public sealed class OnboardingTaskValidationEndpointSendGetOnboardingTaskRules0PortAdapter(ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient client) : IOnboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port
{
    public async Task<ApiBenefits.FindBenefitPlansResponse> SendAsync(ApiBenefits.FindBenefitPlansRequest request, CancellationToken token)
    {
        using var rpc = client.FindBenefitPlansAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
