namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPayslipOverrideReportChecks
{
    public static void StrictPayslipOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705884882);
        var items = new List<StrictPayslipOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictPayslipOverride.NewStrictPayslipOverride($"strict-payslip-override-{index}", "sample"));
        }
        var report = StrictPayslipOverrideReport.BuildStrictPayslipOverrideReport("test", items, clock);
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
        StrictPayslipOverrideReportCountsEachRecordOnce();
    }
}
