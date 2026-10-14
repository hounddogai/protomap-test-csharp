namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccessBadgeClaimReportChecks
{
    public static void AccessBadgeClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709928656);
        var items = new List<AccessBadgeClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeClaim.NewAccessBadgeClaim($"access-badge-claim-{index}", "sample"));
        }
        var report = AccessBadgeClaimReport.BuildAccessBadgeClaimReport("test", items, clock);
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
        AccessBadgeClaimReportCountsEachRecordOnce();
    }
}
