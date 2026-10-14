namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayslipRollupReportChecks
{
    public static void PayslipRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705618608);
        var items = new List<PayslipRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayslipRollup.NewPayslipRollup($"payslip-rollup-{index}", "sample"));
        }
        var report = PayslipRollupReport.BuildPayslipRollupReport("test", items, clock);
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
        PayslipRollupReportCountsEachRecordOnce();
    }
}
