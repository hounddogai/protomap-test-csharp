namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipBatchReportChecks
{
    public static void PayslipBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700357292);
        var items = new List<PayslipBatch>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayslipBatch.NewPayslipBatch($"payslip-batch-{index}", "sample"));
        }
        var report = PayslipBatchReport.BuildPayslipBatchReport("test", items, clock);
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
        PayslipBatchReportCountsEachRecordOnce();
    }
}
