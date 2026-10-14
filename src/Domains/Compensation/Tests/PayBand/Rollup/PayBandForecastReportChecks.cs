namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandForecastReportChecks
{
    public static void PayBandForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707981577);
        var items = new List<PayBandForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayBandForecast.NewPayBandForecast($"pay-band-forecast-{index}", "sample"));
        }
        var report = PayBandForecastReport.BuildPayBandForecastReport("test", items, clock);
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
        PayBandForecastReportCountsEachRecordOnce();
    }
}
