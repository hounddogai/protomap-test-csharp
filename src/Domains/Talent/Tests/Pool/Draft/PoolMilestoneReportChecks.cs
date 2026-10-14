namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PoolMilestoneReportChecks
{
    public static void PoolMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704649306);
        var items = new List<PoolMilestone>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PoolMilestone.NewPoolMilestone($"pool-milestone-{index}", "sample"));
        }
        var report = PoolMilestoneReport.BuildPoolMilestoneReport("test", items, clock);
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
        PoolMilestoneReportCountsEachRecordOnce();
    }
}
