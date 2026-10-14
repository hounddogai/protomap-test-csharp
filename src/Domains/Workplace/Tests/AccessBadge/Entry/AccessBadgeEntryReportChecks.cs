namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeEntryReportChecks
{
    public static void AccessBadgeEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701229877);
        var items = new List<AccessBadgeEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeEntry.NewAccessBadgeEntry($"access-badge-entry-{index}", "sample"));
        }
        var report = AccessBadgeEntryReport.BuildAccessBadgeEntryReport("test", items, clock);
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
        AccessBadgeEntryReportCountsEachRecordOnce();
    }
}
