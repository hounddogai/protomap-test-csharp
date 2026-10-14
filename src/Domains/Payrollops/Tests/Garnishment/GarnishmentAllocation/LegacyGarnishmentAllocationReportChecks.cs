namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentAllocationReportChecks
{
    public static void LegacyGarnishmentAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700262221);
        var items = new List<LegacyGarnishmentAllocation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation($"legacy-garnishment-allocation-{index}", "sample"));
        }
        var report = LegacyGarnishmentAllocationReport.BuildLegacyGarnishmentAllocationReport("test", items, clock);
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
        LegacyGarnishmentAllocationReportCountsEachRecordOnce();
    }
}
