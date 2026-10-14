namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryDeskForecastReportChecks
{
    public static void PrimaryDeskForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705378500);
        var items = new List<PrimaryDeskForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PrimaryDeskForecast.NewPrimaryDeskForecast($"primary-desk-forecast-{index}", "sample"));
        }
        var report = PrimaryDeskForecastReport.BuildPrimaryDeskForecastReport("test", items, clock);
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
        PrimaryDeskForecastReportCountsEachRecordOnce();
    }
}
