namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RetroPaySummaryReportChecks
{
    public static void RetroPaySummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702901522);
        var items = new List<RetroPaySummary>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RetroPaySummary.NewRetroPaySummary($"retro-pay-summary-{index}", "sample"));
        }
        var report = RetroPaySummaryReport.BuildRetroPaySummaryReport("test", items, clock);
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
        RetroPaySummaryReportCountsEachRecordOnce();
    }
}
