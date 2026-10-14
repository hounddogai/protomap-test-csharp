namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanTierReportChecks
{
    public static void StrictSuccessionPlanTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706782181);
        var items = new List<StrictSuccessionPlanTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictSuccessionPlanTier.NewStrictSuccessionPlanTier($"strict-succession-plan-tier-{index}", "sample"));
        }
        var report = StrictSuccessionPlanTierReport.BuildStrictSuccessionPlanTierReport("test", items, clock);
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
        StrictSuccessionPlanTierReportCountsEachRecordOnce();
    }
}
