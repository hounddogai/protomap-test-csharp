namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class LegacyPayslipDigestReportChecks
{
    public static void LegacyPayslipDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702511501);
        var items = new List<LegacyPayslipDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyPayslipDigest.NewLegacyPayslipDigest($"legacy-payslip-digest-{index}", "sample"));
        }
        var report = LegacyPayslipDigestReport.BuildLegacyPayslipDigestReport("test", items, clock);
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
        LegacyPayslipDigestReportCountsEachRecordOnce();
    }
}
