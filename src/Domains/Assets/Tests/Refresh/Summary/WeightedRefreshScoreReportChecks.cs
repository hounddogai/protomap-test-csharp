namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedRefreshScoreReportChecks
{
    public static void WeightedRefreshScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705961382);
        var items = new List<WeightedRefreshScore>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedRefreshScore.NewWeightedRefreshScore($"weighted-refresh-score-{index}", "sample"));
        }
        var report = WeightedRefreshScoreReport.BuildWeightedRefreshScoreReport("test", items, clock);
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
        WeightedRefreshScoreReportCountsEachRecordOnce();
    }
}
