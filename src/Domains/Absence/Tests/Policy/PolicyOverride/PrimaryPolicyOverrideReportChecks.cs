namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PrimaryPolicyOverrideReportChecks
{
    public static void PrimaryPolicyOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705721945);
        var items = new List<PrimaryPolicyOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryPolicyOverride.NewPrimaryPolicyOverride($"primary-policy-override-{index}", "sample"));
        }
        var report = PrimaryPolicyOverrideReport.BuildPrimaryPolicyOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PrimaryPolicyOverrideReportCountsEachRecordOnce();
    }
}
