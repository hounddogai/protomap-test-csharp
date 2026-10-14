namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunSummaryReportChecks
{
    public static void PayRunSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700067695);
        var items = new List<PayRunSummary>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayRunSummary.NewPayRunSummary($"pay-run-summary-{index}", "sample"));
        }
        var report = PayRunSummaryReport.BuildPayRunSummaryReport("test", items, clock);
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
        PayRunSummaryReportCountsEachRecordOnce();
    }
}
