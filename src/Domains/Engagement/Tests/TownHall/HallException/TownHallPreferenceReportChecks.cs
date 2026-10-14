namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallPreferenceReportChecks
{
    public static void TownHallPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709483487);
        var items = new List<TownHallPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TownHallPreference.NewTownHallPreference($"town-hall-preference-{index}", "sample"));
        }
        var report = TownHallPreferenceReport.BuildTownHallPreferenceReport("test", items, clock);
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
        TownHallPreferenceReportCountsEachRecordOnce();
    }
}
