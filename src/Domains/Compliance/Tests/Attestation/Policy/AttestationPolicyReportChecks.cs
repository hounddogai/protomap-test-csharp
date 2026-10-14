namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPolicyReportChecks
{
    public static void AttestationPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705997228);
        var items = new List<AttestationPolicy>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AttestationPolicy.NewAttestationPolicy($"attestation-policy-{index}", "sample"));
        }
        var report = AttestationPolicyReport.BuildAttestationPolicyReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AttestationPolicyReportCountsEachRecordOnce();
    }
}
