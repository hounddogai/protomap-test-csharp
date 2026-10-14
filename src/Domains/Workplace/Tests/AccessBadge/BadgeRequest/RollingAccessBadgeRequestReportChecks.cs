namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class RollingAccessBadgeRequestReportChecks
{
    public static void RollingAccessBadgeRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706718656);
        var items = new List<RollingAccessBadgeRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingAccessBadgeRequest.NewRollingAccessBadgeRequest($"rolling-access-badge-request-{index}", "sample"));
        }
        var report = RollingAccessBadgeRequestReport.BuildRollingAccessBadgeRequestReport("test", items, clock);
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
        RollingAccessBadgeRequestReportCountsEachRecordOnce();
    }
}
