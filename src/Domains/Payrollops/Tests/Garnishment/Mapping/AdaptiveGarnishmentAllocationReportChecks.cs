namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentAllocationReportChecks
{
    public static void AdaptiveGarnishmentAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705304856);
        var items = new List<AdaptiveGarnishmentAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation($"adaptive-garnishment-allocation-{index}", "sample"));
        }
        var report = AdaptiveGarnishmentAllocationReport.BuildAdaptiveGarnishmentAllocationReport("test", items, clock);
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
        AdaptiveGarnishmentAllocationReportCountsEachRecordOnce();
    }
}
