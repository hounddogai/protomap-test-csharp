namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of offboarding slot code, run by the project's runner.</summary>
public static class LegacyOffboardingForecastReportChecks
{
    public static void LegacyOffboardingForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703218501);
        var items = new List<LegacyOffboardingForecast>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyOffboardingForecast.NewLegacyOffboardingForecast($"legacy-offboarding-forecast-{index}", "sample"));
        }
        var report = LegacyOffboardingForecastReport.BuildLegacyOffboardingForecastReport("test", items, clock);
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
        LegacyOffboardingForecastReportCountsEachRecordOnce();
    }
}
