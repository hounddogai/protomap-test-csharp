namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipClaimReportChecks
{
    public static void PayslipClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704862506);
        var items = new List<PayslipClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayslipClaim.NewPayslipClaim($"payslip-claim-{index}", "sample"));
        }
        var report = PayslipClaimReport.BuildPayslipClaimReport("test", items, clock);
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
        PayslipClaimReportCountsEachRecordOnce();
    }
}
