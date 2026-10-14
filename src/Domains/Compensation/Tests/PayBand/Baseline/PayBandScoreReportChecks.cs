namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandScoreReportChecks
{
    public static void PayBandScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700425448);
        var items = new List<PayBandScore>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayBandScore.NewPayBandScore($"pay-band-score-{index}", "sample"));
        }
        var report = PayBandScoreReport.BuildPayBandScoreReport("test", items, clock);
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
        PayBandScoreReportCountsEachRecordOnce();
    }
}
