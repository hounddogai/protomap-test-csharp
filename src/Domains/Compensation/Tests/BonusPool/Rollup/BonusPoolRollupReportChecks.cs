namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolRollupReportChecks
{
    public static void BonusPoolRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708512399);
        var items = new List<BonusPoolRollup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BonusPoolRollup.NewBonusPoolRollup($"bonus-pool-rollup-{index}", "sample"));
        }
        var report = BonusPoolRollupReport.BuildBonusPoolRollupReport("test", items, clock);
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
        BonusPoolRollupReportCountsEachRecordOnce();
    }
}
