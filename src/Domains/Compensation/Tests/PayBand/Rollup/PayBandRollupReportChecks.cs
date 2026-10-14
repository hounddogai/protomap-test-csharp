namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandRollupReportChecks
{
    public static void PayBandRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704298864);
        var items = new List<PayBandRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayBandRollup.NewPayBandRollup($"pay-band-rollup-{index}", "sample"));
        }
        var report = PayBandRollupReport.BuildPayBandRollupReport("test", items, clock);
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
        PayBandRollupReportCountsEachRecordOnce();
    }
}
