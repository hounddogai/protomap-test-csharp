namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientOffboardingForecastReportChecks
{
    public static void LenientOffboardingForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700754477);
        var items = new List<LenientOffboardingForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientOffboardingForecast.NewLenientOffboardingForecast($"lenient-offboarding-forecast-{index}", "sample"));
        }
        var report = LenientOffboardingForecastReport.BuildLenientOffboardingForecastReport("test", items, clock);
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
        LenientOffboardingForecastReportCountsEachRecordOnce();
    }
}
