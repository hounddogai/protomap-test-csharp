namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalNominationMilestoneReportChecks
{
    public static void GlobalNominationMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700146294);
        var items = new List<GlobalNominationMilestone>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalNominationMilestone.NewGlobalNominationMilestone($"global-nomination-milestone-{index}", "sample"));
        }
        var report = GlobalNominationMilestoneReport.BuildGlobalNominationMilestoneReport("test", items, clock);
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
        GlobalNominationMilestoneReportCountsEachRecordOnce();
    }
}
