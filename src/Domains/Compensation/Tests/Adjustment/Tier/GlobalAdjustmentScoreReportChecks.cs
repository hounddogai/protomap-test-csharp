namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GlobalAdjustmentScoreReportChecks
{
    public static void GlobalAdjustmentScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703041682);
        var items = new List<GlobalAdjustmentScore>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalAdjustmentScore.NewGlobalAdjustmentScore($"global-adjustment-score-{index}", "sample"));
        }
        var report = GlobalAdjustmentScoreReport.BuildGlobalAdjustmentScoreReport("test", items, clock);
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
        GlobalAdjustmentScoreReportCountsEachRecordOnce();
    }
}
