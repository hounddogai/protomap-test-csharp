namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshScoreReportChecks
{
    public static void RefreshScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709979200);
        var items = new List<RefreshScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RefreshScore.NewRefreshScore($"refresh-score-{index}", "sample"));
        }
        var report = RefreshScoreReport.BuildRefreshScoreReport("test", items, clock);
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
        RefreshScoreReportCountsEachRecordOnce();
    }
}
