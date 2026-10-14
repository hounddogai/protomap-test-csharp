namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedTownHallExceptionReportChecks
{
    public static void WeightedTownHallExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702594629);
        var items = new List<WeightedTownHallException>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedTownHallException.NewWeightedTownHallException($"weighted-town-hall-exception-{index}", "sample"));
        }
        var report = WeightedTownHallExceptionReport.BuildWeightedTownHallExceptionReport("test", items, clock);
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
        WeightedTownHallExceptionReportCountsEachRecordOnce();
    }
}
