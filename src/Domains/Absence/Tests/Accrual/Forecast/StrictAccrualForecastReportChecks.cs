namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class StrictAccrualForecastReportChecks
{
    public static void StrictAccrualForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707594100);
        var items = new List<StrictAccrualForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictAccrualForecast.NewStrictAccrualForecast($"strict-accrual-forecast-{index}", "sample"));
        }
        var report = StrictAccrualForecastReport.BuildStrictAccrualForecastReport("test", items, clock);
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
        StrictAccrualForecastReportCountsEachRecordOnce();
    }
}
