namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledTownHallOverrideReportChecks
{
    public static void ScheduledTownHallOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705256227);
        var items = new List<ScheduledTownHallOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledTownHallOverride.NewScheduledTownHallOverride($"scheduled-town-hall-override-{index}", "sample"));
        }
        var report = ScheduledTownHallOverrideReport.BuildScheduledTownHallOverrideReport("test", items, clock);
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
        ScheduledTownHallOverrideReportCountsEachRecordOnce();
    }
}
