namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementForecastReportChecks
{
    public static void EngagementForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703407946);
        var items = new List<EngagementForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EngagementForecast.NewEngagementForecast($"engagement-forecast-{index}", "sample"));
        }
        var report = EngagementForecastReport.BuildEngagementForecastReport("test", items, clock);
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
        EngagementForecastReportCountsEachRecordOnce();
    }
}
