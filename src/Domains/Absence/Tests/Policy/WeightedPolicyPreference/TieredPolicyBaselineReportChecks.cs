namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class TieredPolicyBaselineReportChecks
{
    public static void TieredPolicyBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709335884);
        var items = new List<TieredPolicyBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredPolicyBaseline.NewTieredPolicyBaseline($"tiered-policy-baseline-{index}", "sample"));
        }
        var report = TieredPolicyBaselineReport.BuildTieredPolicyBaselineReport("test", items, clock);
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
        TieredPolicyBaselineReportCountsEachRecordOnce();
    }
}
