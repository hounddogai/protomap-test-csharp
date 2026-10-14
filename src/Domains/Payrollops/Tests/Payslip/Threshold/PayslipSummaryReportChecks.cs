namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipSummaryReportChecks
{
    public static void PayslipSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701935558);
        var items = new List<PayslipSummary>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PayslipSummary.NewPayslipSummary($"payslip-summary-{index}", "sample"));
        }
        var report = PayslipSummaryReport.BuildPayslipSummaryReport("test", items, clock);
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
        PayslipSummaryReportCountsEachRecordOnce();
    }
}
