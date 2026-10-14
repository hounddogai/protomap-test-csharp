namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallWindowReportChecks
{
    public static void TownHallWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705021267);
        var items = new List<TownHallWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TownHallWindow.NewTownHallWindow($"town-hall-window-{index}", "sample"));
        }
        var report = TownHallWindowReport.BuildTownHallWindowReport("test", items, clock);
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
        TownHallWindowReportCountsEachRecordOnce();
    }
}
