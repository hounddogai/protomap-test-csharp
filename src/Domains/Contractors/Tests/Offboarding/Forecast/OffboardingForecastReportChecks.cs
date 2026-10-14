namespace Acme.Workforce.Contractors.Tests.Offboarding.Forecast;

using Acme.Workforce.Contractors.Offboarding.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class OffboardingForecastReportChecks
{
    public static void OffboardingForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708312109);
        var items = new List<OffboardingForecast>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(OffboardingForecast.NewOffboardingForecast($"offboarding-forecast-{index}", "sample"));
        }
        var report = OffboardingForecastReport.BuildOffboardingForecastReport("test", items, clock);
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
        OffboardingForecastReportCountsEachRecordOnce();
    }
}
