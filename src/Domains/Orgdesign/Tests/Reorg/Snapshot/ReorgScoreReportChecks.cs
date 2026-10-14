namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgScoreReportChecks
{
    public static void ReorgScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709991767);
        var items = new List<ReorgScore>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ReorgScore.NewReorgScore($"reorg-score-{index}", "sample"));
        }
        var report = ReorgScoreReport.BuildReorgScoreReport("test", items, clock);
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
        ReorgScoreReportCountsEachRecordOnce();
    }
}
