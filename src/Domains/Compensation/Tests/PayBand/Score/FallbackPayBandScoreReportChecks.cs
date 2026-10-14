namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FallbackPayBandScoreReportChecks
{
    public static void FallbackPayBandScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702664426);
        var items = new List<FallbackPayBandScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FallbackPayBandScore.NewFallbackPayBandScore($"fallback-pay-band-score-{index}", "sample"));
        }
        var report = FallbackPayBandScoreReport.BuildFallbackPayBandScoreReport("test", items, clock);
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
        FallbackPayBandScoreReportCountsEachRecordOnce();
    }
}
