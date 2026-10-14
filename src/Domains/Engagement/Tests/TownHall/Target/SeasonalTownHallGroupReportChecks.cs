namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalTownHallGroupReportChecks
{
    public static void SeasonalTownHallGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701433723);
        var items = new List<SeasonalTownHallGroup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalTownHallGroup.NewSeasonalTownHallGroup($"seasonal-town-hall-group-{index}", "sample"));
        }
        var report = SeasonalTownHallGroupReport.BuildSeasonalTownHallGroupReport("test", items, clock);
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
        SeasonalTownHallGroupReportCountsEachRecordOnce();
    }
}
