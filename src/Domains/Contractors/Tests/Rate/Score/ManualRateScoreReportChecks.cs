namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ManualRateScoreReportChecks
{
    public static void ManualRateScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709931028);
        var items = new List<ManualRateScore>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualRateScore.NewManualRateScore($"manual-rate-score-{index}", "sample"));
        }
        var report = ManualRateScoreReport.BuildManualRateScoreReport("test", items, clock);
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
        ManualRateScoreReportCountsEachRecordOnce();
    }
}
