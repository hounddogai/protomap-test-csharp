namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolWindowReportChecks
{
    public static void BonusPoolWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702239278);
        var items = new List<BonusPoolWindow>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BonusPoolWindow.NewBonusPoolWindow($"bonus-pool-window-{index}", "sample"));
        }
        var report = BonusPoolWindowReport.BuildBonusPoolWindowReport("test", items, clock);
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
        BonusPoolWindowReportCountsEachRecordOnce();
    }
}
