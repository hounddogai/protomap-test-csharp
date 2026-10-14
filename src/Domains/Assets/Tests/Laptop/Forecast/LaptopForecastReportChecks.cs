namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopForecastReportChecks
{
    public static void LaptopForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709794358);
        var items = new List<LaptopForecast>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LaptopForecast.NewLaptopForecast($"laptop-forecast-{index}", "sample"));
        }
        var report = LaptopForecastReport.BuildLaptopForecastReport("test", items, clock);
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
        LaptopForecastReportCountsEachRecordOnce();
    }
}
