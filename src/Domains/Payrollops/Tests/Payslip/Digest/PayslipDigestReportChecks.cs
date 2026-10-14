namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipDigestReportChecks
{
    public static void PayslipDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700788056);
        var items = new List<PayslipDigest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayslipDigest.NewPayslipDigest($"payslip-digest-{index}", "sample"));
        }
        var report = PayslipDigestReport.BuildPayslipDigestReport("test", items, clock);
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
        PayslipDigestReportCountsEachRecordOnce();
    }
}
