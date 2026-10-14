namespace Acme.Workforce.Payrollops.Tests.PayRun.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingPayRunAllocationReportChecks
{
    public static void PendingPayRunAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709552812);
        var items = new List<PendingPayRunAllocation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingPayRunAllocation.NewPendingPayRunAllocation($"pending-pay-run-allocation-{index}", "sample"));
        }
        var report = PendingPayRunAllocationReport.BuildPendingPayRunAllocationReport("test", items, clock);
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
        PendingPayRunAllocationReportCountsEachRecordOnce();
    }
}
