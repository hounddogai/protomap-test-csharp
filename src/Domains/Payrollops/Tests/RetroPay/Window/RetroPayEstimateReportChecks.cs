namespace Acme.Workforce.Payrollops.Tests.RetroPay.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RetroPayEstimateReportChecks
{
    public static void RetroPayEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703349302);
        var items = new List<RetroPayEstimate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RetroPayEstimate.NewRetroPayEstimate($"retro-pay-estimate-{index}", "sample"));
        }
        var report = RetroPayEstimateReport.BuildRetroPayEstimateReport("test", items, clock);
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
        RetroPayEstimateReportCountsEachRecordOnce();
    }
}
