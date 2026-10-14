namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class BatchedPayslipClaimReportChecks
{
    public static void BatchedPayslipClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700878125);
        var items = new List<BatchedPayslipClaim>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedPayslipClaim.NewBatchedPayslipClaim($"batched-payslip-claim-{index}", "sample"));
        }
        var report = BatchedPayslipClaimReport.BuildBatchedPayslipClaimReport("test", items, clock);
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
        BatchedPayslipClaimReportCountsEachRecordOnce();
    }
}
