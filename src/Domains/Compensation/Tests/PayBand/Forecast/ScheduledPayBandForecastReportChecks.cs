namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledPayBandForecastReportChecks
{
    public static void ScheduledPayBandForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709002234);
        var items = new List<ScheduledPayBandForecast>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledPayBandForecast.NewScheduledPayBandForecast($"scheduled-pay-band-forecast-{index}", "sample"));
        }
        var report = ScheduledPayBandForecastReport.BuildScheduledPayBandForecastReport("test", items, clock);
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
        ScheduledPayBandForecastReportCountsEachRecordOnce();
    }
}
