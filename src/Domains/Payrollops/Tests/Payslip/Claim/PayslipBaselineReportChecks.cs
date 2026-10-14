namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipBaselineReportChecks
{
    public static void PayslipBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706342375);
        var items = new List<PayslipBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PayslipBaseline.NewPayslipBaseline($"payslip-baseline-{index}", "sample"));
        }
        var report = PayslipBaselineReport.BuildPayslipBaselineReport("test", items, clock);
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
        PayslipBaselineReportCountsEachRecordOnce();
    }
}
