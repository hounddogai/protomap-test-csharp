namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipEstimateReportChecks
{
    public static void PayslipEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700578388);
        var items = new List<PayslipEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PayslipEstimate.NewPayslipEstimate($"payslip-estimate-{index}", "sample"));
        }
        var report = PayslipEstimateReport.BuildPayslipEstimateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PayslipEstimateReportCountsEachRecordOnce();
    }
}
