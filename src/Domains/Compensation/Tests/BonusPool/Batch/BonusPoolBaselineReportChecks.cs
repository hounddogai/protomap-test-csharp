namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolBaselineReportChecks
{
    public static void BonusPoolBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703699791);
        var items = new List<BonusPoolBaseline>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BonusPoolBaseline.NewBonusPoolBaseline($"bonus-pool-baseline-{index}", "sample"));
        }
        var report = BonusPoolBaselineReport.BuildBonusPoolBaselineReport("test", items, clock);
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
        BonusPoolBaselineReportCountsEachRecordOnce();
    }
}
