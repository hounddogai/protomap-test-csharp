namespace Acme.Workforce.Assets.Tests.LicenseSeat.Forecast;

using Acme.Workforce.Assets.LicenseSeat.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LicenseSeatForecastReportChecks
{
    public static void LicenseSeatForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700229015);
        var items = new List<LicenseSeatForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LicenseSeatForecast.NewLicenseSeatForecast($"license-seat-forecast-{index}", "sample"));
        }
        var report = LicenseSeatForecastReport.BuildLicenseSeatForecastReport("test", items, clock);
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
        LicenseSeatForecastReportCountsEachRecordOnce();
    }
}
