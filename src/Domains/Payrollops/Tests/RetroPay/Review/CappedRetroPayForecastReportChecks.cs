namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedRetroPayForecastReportChecks
{
    public static void CappedRetroPayForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702210833);
        var items = new List<CappedRetroPayForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CappedRetroPayForecast.NewCappedRetroPayForecast($"capped-retro-pay-forecast-{index}", "sample"));
        }
        var report = CappedRetroPayForecastReport.BuildCappedRetroPayForecastReport("test", items, clock);
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
        CappedRetroPayForecastReportCountsEachRecordOnce();
    }
}
