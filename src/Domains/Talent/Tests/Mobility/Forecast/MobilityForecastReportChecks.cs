namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityForecastReportChecks
{
    public static void MobilityForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708798625);
        var items = new List<MobilityForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MobilityForecast.NewMobilityForecast($"mobility-forecast-{index}", "sample"));
        }
        var report = MobilityForecastReport.BuildMobilityForecastReport("test", items, clock);
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
        MobilityForecastReportCountsEachRecordOnce();
    }
}
