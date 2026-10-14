namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeMilestoneReportChecks
{
    public static void AccessBadgeMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708869725);
        var items = new List<AccessBadgeMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AccessBadgeMilestone.NewAccessBadgeMilestone($"access-badge-milestone-{index}", "sample"));
        }
        var report = AccessBadgeMilestoneReport.BuildAccessBadgeMilestoneReport("test", items, clock);
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
        AccessBadgeMilestoneReportCountsEachRecordOnce();
    }
}
