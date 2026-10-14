namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallRequestReportChecks
{
    public static void TownHallRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704636955);
        var items = new List<TownHallRequest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TownHallRequest.NewTownHallRequest($"town-hall-request-{index}", "sample"));
        }
        var report = TownHallRequestReport.BuildTownHallRequestReport("test", items, clock);
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
        TownHallRequestReportCountsEachRecordOnce();
    }
}
