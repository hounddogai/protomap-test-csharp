namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SeasonalMobilityForecastReportChecks
{
    public static void SeasonalMobilityForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704935846);
        var items = new List<SeasonalMobilityForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SeasonalMobilityForecast.NewSeasonalMobilityForecast($"seasonal-mobility-forecast-{index}", "sample"));
        }
        var report = SeasonalMobilityForecastReport.BuildSeasonalMobilityForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalMobilityForecastReportCountsEachRecordOnce();
    }
}
