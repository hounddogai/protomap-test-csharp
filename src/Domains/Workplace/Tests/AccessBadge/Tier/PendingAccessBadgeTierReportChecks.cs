namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingAccessBadgeTierReportChecks
{
    public static void PendingAccessBadgeTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709968176);
        var items = new List<PendingAccessBadgeTier>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingAccessBadgeTier.NewPendingAccessBadgeTier($"pending-access-badge-tier-{index}", "sample"));
        }
        var report = PendingAccessBadgeTierReport.BuildPendingAccessBadgeTierReport("test", items, clock);
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
        PendingAccessBadgeTierReportCountsEachRecordOnce();
    }
}
