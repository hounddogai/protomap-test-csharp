namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class SeasonalGarnishmentAllocationReportChecks
{
    public static void SeasonalGarnishmentAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706765452);
        var items = new List<SeasonalGarnishmentAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation($"seasonal-garnishment-allocation-{index}", "sample"));
        }
        var report = SeasonalGarnishmentAllocationReport.BuildSeasonalGarnishmentAllocationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentAllocationReportCountsEachRecordOnce();
    }
}
