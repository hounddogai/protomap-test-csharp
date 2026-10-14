namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultRetroPayAllocationReportChecks
{
    public static void DefaultRetroPayAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705144179);
        var items = new List<DefaultRetroPayAllocation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultRetroPayAllocation.NewDefaultRetroPayAllocation($"default-retro-pay-allocation-{index}", "sample"));
        }
        var report = DefaultRetroPayAllocationReport.BuildDefaultRetroPayAllocationReport("test", items, clock);
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
        DefaultRetroPayAllocationReportCountsEachRecordOnce();
    }
}
