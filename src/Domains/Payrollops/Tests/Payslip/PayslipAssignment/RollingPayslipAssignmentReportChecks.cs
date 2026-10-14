namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class RollingPayslipAssignmentReportChecks
{
    public static void RollingPayslipAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703736815);
        var items = new List<RollingPayslipAssignment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingPayslipAssignment.NewRollingPayslipAssignment($"rolling-payslip-assignment-{index}", "sample"));
        }
        var report = RollingPayslipAssignmentReport.BuildRollingPayslipAssignmentReport("test", items, clock);
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
        RollingPayslipAssignmentReportCountsEachRecordOnce();
    }
}
