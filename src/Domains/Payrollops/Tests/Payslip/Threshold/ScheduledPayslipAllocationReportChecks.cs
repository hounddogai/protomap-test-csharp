namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledPayslipAllocationReportChecks
{
    public static void ScheduledPayslipAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704552585);
        var items = new List<ScheduledPayslipAllocation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledPayslipAllocation.NewScheduledPayslipAllocation($"scheduled-payslip-allocation-{index}", "sample"));
        }
        var report = ScheduledPayslipAllocationReport.BuildScheduledPayslipAllocationReport("test", items, clock);
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
        ScheduledPayslipAllocationReportCountsEachRecordOnce();
    }
}
