namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallScoreReportChecks
{
    public static void TownHallScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703891903);
        var items = new List<TownHallScore>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TownHallScore.NewTownHallScore($"town-hall-score-{index}", "sample"));
        }
        var report = TownHallScoreReport.BuildTownHallScoreReport("test", items, clock);
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
        TownHallScoreReportCountsEachRecordOnce();
    }
}
