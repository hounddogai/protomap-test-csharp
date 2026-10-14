namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class ScheduledTownHallExceptionReportChecks
{
    public static void ScheduledTownHallExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702449662);
        var items = new List<ScheduledTownHallException>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledTownHallException.NewScheduledTownHallException($"scheduled-town-hall-exception-{index}", "sample"));
        }
        var report = ScheduledTownHallExceptionReport.BuildScheduledTownHallExceptionReport("test", items, clock);
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
        ScheduledTownHallExceptionReportCountsEachRecordOnce();
    }
}
