namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationOverrideReportChecks
{
    public static void AttestationOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707198452);
        var items = new List<AttestationOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AttestationOverride.NewAttestationOverride($"attestation-override-{index}", "sample"));
        }
        var report = AttestationOverrideReport.BuildAttestationOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AttestationOverrideReportCountsEachRecordOnce();
    }
}
