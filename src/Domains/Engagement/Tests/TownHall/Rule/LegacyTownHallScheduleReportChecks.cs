namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallScheduleReportChecks
{
    public static void LegacyTownHallScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709440776);
        var items = new List<LegacyTownHallSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyTownHallSchedule.NewLegacyTownHallSchedule($"legacy-town-hall-schedule-{index}", "sample"));
        }
        var report = LegacyTownHallScheduleReport.BuildLegacyTownHallScheduleReport("test", items, clock);
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
        LegacyTownHallScheduleReportCountsEachRecordOnce();
    }
}
