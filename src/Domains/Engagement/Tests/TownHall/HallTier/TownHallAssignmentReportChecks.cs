namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallAssignmentReportChecks
{
    public static void TownHallAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705209563);
        var items = new List<TownHallAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TownHallAssignment.NewTownHallAssignment($"town-hall-assignment-{index}", "sample"));
        }
        var report = TownHallAssignmentReport.BuildTownHallAssignmentReport("test", items, clock);
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
        TownHallAssignmentReportCountsEachRecordOnce();
    }
}
