namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedHeadcountWindowReportChecks
{
    public static void WeightedHeadcountWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708396332);
        var items = new List<WeightedHeadcountWindow>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedHeadcountWindow.NewWeightedHeadcountWindow($"weighted-headcount-window-{index}", "sample"));
        }
        var report = WeightedHeadcountWindowReport.BuildWeightedHeadcountWindowReport("test", items, clock);
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
        WeightedHeadcountWindowReportCountsEachRecordOnce();
    }
}
