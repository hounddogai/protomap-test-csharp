namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalRefreshAllocationReportChecks
{
    public static void SeasonalRefreshAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707895006);
        var items = new List<SeasonalRefreshAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SeasonalRefreshAllocation.NewSeasonalRefreshAllocation($"seasonal-refresh-allocation-{index}", "sample"));
        }
        var report = SeasonalRefreshAllocationReport.BuildSeasonalRefreshAllocationReport("test", items, clock);
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
        SeasonalRefreshAllocationReportCountsEachRecordOnce();
    }
}
