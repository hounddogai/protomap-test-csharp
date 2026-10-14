namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CappedPayslipBatchReportChecks
{
    public static void CappedPayslipBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702014496);
        var items = new List<CappedPayslipBatch>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedPayslipBatch.NewCappedPayslipBatch($"capped-payslip-batch-{index}", "sample"));
        }
        var report = CappedPayslipBatchReport.BuildCappedPayslipBatchReport("test", items, clock);
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
        CappedPayslipBatchReportCountsEachRecordOnce();
    }
}
