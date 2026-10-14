namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualForecastReportChecks
{
    public static void AccrualForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701345114);
        var items = new List<AccrualForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccrualForecast.NewAccrualForecast($"accrual-forecast-{index}", "sample"));
        }
        var report = AccrualForecastReport.BuildAccrualForecastReport("test", items, clock);
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
        AccrualForecastReportCountsEachRecordOnce();
    }
}
