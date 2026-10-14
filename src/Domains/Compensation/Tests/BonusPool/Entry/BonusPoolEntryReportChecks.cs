namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolEntryReportChecks
{
    public static void BonusPoolEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704951626);
        var items = new List<BonusPoolEntry>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BonusPoolEntry.NewBonusPoolEntry($"bonus-pool-entry-{index}", "sample"));
        }
        var report = BonusPoolEntryReport.BuildBonusPoolEntryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BonusPoolEntryReportCountsEachRecordOnce();
    }
}
