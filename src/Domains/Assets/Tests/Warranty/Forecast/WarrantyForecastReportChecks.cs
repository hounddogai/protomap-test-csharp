namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyForecastReportChecks
{
    public static void WarrantyForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709125259);
        var items = new List<WarrantyForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WarrantyForecast.NewWarrantyForecast($"warranty-forecast-{index}", "sample"));
        }
        var report = WarrantyForecastReport.BuildWarrantyForecastReport("test", items, clock);
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
        WarrantyForecastReportCountsEachRecordOnce();
    }
}
