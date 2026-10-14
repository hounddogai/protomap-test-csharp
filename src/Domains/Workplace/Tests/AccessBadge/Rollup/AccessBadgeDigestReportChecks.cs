namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeDigestReportChecks
{
    public static void AccessBadgeDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705683216);
        var items = new List<AccessBadgeDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeDigest.NewAccessBadgeDigest($"access-badge-digest-{index}", "sample"));
        }
        var report = AccessBadgeDigestReport.BuildAccessBadgeDigestReport("test", items, clock);
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
        AccessBadgeDigestReportCountsEachRecordOnce();
    }
}
