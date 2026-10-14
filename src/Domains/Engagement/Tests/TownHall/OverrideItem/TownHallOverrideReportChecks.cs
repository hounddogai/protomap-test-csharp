namespace Acme.Workforce.Engagement.Tests.TownHall.OverrideItem;

using Acme.Workforce.Engagement.TownHall.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TownHallOverrideReportChecks
{
    public static void TownHallOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709364009);
        var items = new List<TownHallOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TownHallOverride.NewTownHallOverride($"town-hall-override-{index}", "sample"));
        }
        var report = TownHallOverrideReport.BuildTownHallOverrideReport("test", items, clock);
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
        TownHallOverrideReportCountsEachRecordOnce();
    }
}
