namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DefaultPayslipEntryReportChecks
{
    public static void DefaultPayslipEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708216450);
        var items = new List<DefaultPayslipEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultPayslipEntry.NewDefaultPayslipEntry($"default-payslip-entry-{index}", "sample"));
        }
        var report = DefaultPayslipEntryReport.BuildDefaultPayslipEntryReport("test", items, clock);
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
        DefaultPayslipEntryReportCountsEachRecordOnce();
    }
}
