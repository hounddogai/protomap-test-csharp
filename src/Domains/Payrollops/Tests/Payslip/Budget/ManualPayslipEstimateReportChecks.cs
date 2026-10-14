namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ManualPayslipEstimateReportChecks
{
    public static void ManualPayslipEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709654789);
        var items = new List<ManualPayslipEstimate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualPayslipEstimate.NewManualPayslipEstimate($"manual-payslip-estimate-{index}", "sample"));
        }
        var report = ManualPayslipEstimateReport.BuildManualPayslipEstimateReport("test", items, clock);
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
        ManualPayslipEstimateReportCountsEachRecordOnce();
    }
}
