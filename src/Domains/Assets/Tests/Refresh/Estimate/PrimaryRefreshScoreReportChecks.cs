namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshScoreReportChecks
{
    public static void PrimaryRefreshScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707988319);
        var items = new List<PrimaryRefreshScore>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryRefreshScore.NewPrimaryRefreshScore($"primary-refresh-score-{index}", "sample"));
        }
        var report = PrimaryRefreshScoreReport.BuildPrimaryRefreshScoreReport("test", items, clock);
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
        PrimaryRefreshScoreReportCountsEachRecordOnce();
    }
}
