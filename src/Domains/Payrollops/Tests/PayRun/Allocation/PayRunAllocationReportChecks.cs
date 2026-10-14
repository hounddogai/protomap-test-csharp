namespace Acme.Workforce.Payrollops.Tests.PayRun.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PayRunAllocationReportChecks
{
    public static void PayRunAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705685021);
        var items = new List<PayRunAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PayRunAllocation.NewPayRunAllocation($"pay-run-allocation-{index}", "sample"));
        }
        var report = PayRunAllocationReport.BuildPayRunAllocationReport("test", items, clock);
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
        PayRunAllocationReportCountsEachRecordOnce();
    }
}
