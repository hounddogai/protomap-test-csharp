namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolRecordReportChecks
{
    public static void BonusPoolRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704523999);
        var items = new List<BonusPoolRecord>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BonusPoolRecord.NewBonusPoolRecord($"bonus-pool-record-{index}", "sample"));
        }
        var report = BonusPoolRecordReport.BuildBonusPoolRecordReport("test", items, clock);
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
        BonusPoolRecordReportCountsEachRecordOnce();
    }
}
