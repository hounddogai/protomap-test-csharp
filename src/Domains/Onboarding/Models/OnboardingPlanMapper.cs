using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The onboarding plan snapshot persisted by the domain repository.
/// </summary>
public sealed record OnboardingPlanSnapshot(string TenantId, string Id, byte[] Payload);

public static class OnboardingPlanMapper
{
    public static StoredRecord ToRecord(string tenant, ApiOnboarding.OnboardingPlan value)
    {
        var snapshot = new OnboardingPlanSnapshot(tenant, value.OnboardingPlanId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "onboarding_plan", snapshot.Id, snapshot.Payload);
    }
    public static ApiOnboarding.OnboardingPlan FromRecord(StoredRecord record) => ApiOnboarding.OnboardingPlan.Parser.ParseFrom(record.Payload);
}
