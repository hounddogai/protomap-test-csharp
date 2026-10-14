namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeForecastReportChecks
{
    public static void AccessBadgeForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700498451);
        var items = new List<AccessBadgeForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeForecast.NewAccessBadgeForecast($"access-badge-forecast-{index}", "sample"));
        }
        var report = AccessBadgeForecastReport.BuildAccessBadgeForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AccessBadgeForecastReportCountsEachRecordOnce();
    }
}
