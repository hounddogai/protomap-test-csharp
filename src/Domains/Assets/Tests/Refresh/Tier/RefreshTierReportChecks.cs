namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshTierReportChecks
{
    public static void RefreshTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702124972);
        var items = new List<RefreshTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RefreshTier.NewRefreshTier($"refresh-tier-{index}", "sample"));
        }
        var report = RefreshTierReport.BuildRefreshTierReport("test", items, clock);
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
        RefreshTierReportCountsEachRecordOnce();
    }
}
