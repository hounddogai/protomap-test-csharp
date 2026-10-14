namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictPoolMilestoneReportChecks
{
    public static void StrictPoolMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708940808);
        var items = new List<StrictPoolMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictPoolMilestone.NewStrictPoolMilestone($"strict-pool-milestone-{index}", "sample"));
        }
        var report = StrictPoolMilestoneReport.BuildStrictPoolMilestoneReport("test", items, clock);
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
        StrictPoolMilestoneReportCountsEachRecordOnce();
    }
}
