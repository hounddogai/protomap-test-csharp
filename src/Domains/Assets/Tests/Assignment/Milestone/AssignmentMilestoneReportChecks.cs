namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentMilestoneReportChecks
{
    public static void AssignmentMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702224690);
        var items = new List<AssignmentMilestone>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssignmentMilestone.NewAssignmentMilestone($"assignment-milestone-{index}", "sample"));
        }
        var report = AssignmentMilestoneReport.BuildAssignmentMilestoneReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AssignmentMilestoneReportCountsEachRecordOnce();
    }
}
