namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TieredPayBandScoreReportChecks
{
    public static void TieredPayBandScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703788172);
        var items = new List<TieredPayBandScore>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredPayBandScore.NewTieredPayBandScore($"tiered-pay-band-score-{index}", "sample"));
        }
        var report = TieredPayBandScoreReport.BuildTieredPayBandScoreReport("test", items, clock);
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
        TieredPayBandScoreReportCountsEachRecordOnce();
    }
}
