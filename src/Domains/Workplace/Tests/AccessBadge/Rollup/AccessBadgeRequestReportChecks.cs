namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeRequestReportChecks
{
    public static void AccessBadgeRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703821638);
        var items = new List<AccessBadgeRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AccessBadgeRequest.NewAccessBadgeRequest($"access-badge-request-{index}", "sample"));
        }
        var report = AccessBadgeRequestReport.BuildAccessBadgeRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AccessBadgeRequestReportCountsEachRecordOnce();
    }
}
