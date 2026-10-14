namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingTownHallThresholdReportChecks
{
    public static void RollingTownHallThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702289597);
        var items = new List<RollingTownHallThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RollingTownHallThreshold.NewRollingTownHallThreshold($"rolling-town-hall-threshold-{index}", "sample"));
        }
        var report = RollingTownHallThresholdReport.BuildRollingTownHallThresholdReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RollingTownHallThresholdReportCountsEachRecordOnce();
    }
}
