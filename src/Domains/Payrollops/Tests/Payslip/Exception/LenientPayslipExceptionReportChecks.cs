namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientPayslipExceptionReportChecks
{
    public static void LenientPayslipExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709051408);
        var items = new List<LenientPayslipException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientPayslipException.NewLenientPayslipException($"lenient-payslip-exception-{index}", "sample"));
        }
        var report = LenientPayslipExceptionReport.BuildLenientPayslipExceptionReport("test", items, clock);
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
        LenientPayslipExceptionReportCountsEachRecordOnce();
    }
}
