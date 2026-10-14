namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipTargetReportChecks
{
    public static void CappedPayslipTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703112139);
        var items = new List<CappedPayslipTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CappedPayslipTarget.NewCappedPayslipTarget($"capped-payslip-target-{index}", "sample"));
        }
        var report = CappedPayslipTargetReport.BuildCappedPayslipTargetReport("test", items, clock);
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
        CappedPayslipTargetReportCountsEachRecordOnce();
    }
}
