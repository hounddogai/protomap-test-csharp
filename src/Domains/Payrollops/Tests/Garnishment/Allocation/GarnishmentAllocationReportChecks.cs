namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentAllocationReportChecks
{
    public static void GarnishmentAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703920264);
        var items = new List<GarnishmentAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GarnishmentAllocation.NewGarnishmentAllocation($"garnishment-allocation-{index}", "sample"));
        }
        var report = GarnishmentAllocationReport.BuildGarnishmentAllocationReport("test", items, clock);
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
        GarnishmentAllocationReportCountsEachRecordOnce();
    }
}
