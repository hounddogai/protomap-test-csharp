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
/// The onboarding task snapshot persisted by the domain repository.
/// </summary>
public sealed record OnboardingTaskSnapshot(string TenantId, string Id, byte[] Payload);

public static class OnboardingTaskMapper
{
    public static StoredRecord ToRecord(string tenant, ApiOnboarding.OnboardingTask value)
    {
        var snapshot = new OnboardingTaskSnapshot(tenant, value.OnboardingTaskId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "onboarding_task", snapshot.Id, snapshot.Payload);
    }
    public static ApiOnboarding.OnboardingTask FromRecord(StoredRecord record) => ApiOnboarding.OnboardingTask.Parser.ParseFrom(record.Payload);
}
