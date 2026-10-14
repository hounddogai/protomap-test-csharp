namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalHeadcountWindowReportChecks
{
    public static void SeasonalHeadcountWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702563250);
        var items = new List<SeasonalHeadcountWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalHeadcountWindow.NewSeasonalHeadcountWindow($"seasonal-headcount-window-{index}", "sample"));
        }
        var report = SeasonalHeadcountWindowReport.BuildSeasonalHeadcountWindowReport("test", items, clock);
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
        SeasonalHeadcountWindowReportCountsEachRecordOnce();
    }
}
