namespace Acme.Workforce.Absence.Tests.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ReturnPlanForecastReportChecks
{
    public static void ReturnPlanForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700893741);
        var items = new List<ReturnPlanForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ReturnPlanForecast.NewReturnPlanForecast($"return-plan-forecast-{index}", "sample"));
        }
        var report = ReturnPlanForecastReport.BuildReturnPlanForecastReport("test", items, clock);
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
        ReturnPlanForecastReportCountsEachRecordOnce();
    }
}
