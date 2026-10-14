namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeProfileReportChecks
{
    public static void AccessBadgeProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705065608);
        var items = new List<AccessBadgeProfile>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeProfile.NewAccessBadgeProfile($"access-badge-profile-{index}", "sample"));
        }
        var report = AccessBadgeProfileReport.BuildAccessBadgeProfileReport("test", items, clock);
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
        AccessBadgeProfileReportCountsEachRecordOnce();
    }
}
