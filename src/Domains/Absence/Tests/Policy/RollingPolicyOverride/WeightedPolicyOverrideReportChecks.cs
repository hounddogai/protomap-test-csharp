namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class WeightedPolicyOverrideReportChecks
{
    public static void WeightedPolicyOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700705190);
        var items = new List<WeightedPolicyOverride>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedPolicyOverride.NewWeightedPolicyOverride($"weighted-policy-override-{index}", "sample"));
        }
        var report = WeightedPolicyOverrideReport.BuildWeightedPolicyOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedPolicyOverrideReportCountsEachRecordOnce();
    }
}
