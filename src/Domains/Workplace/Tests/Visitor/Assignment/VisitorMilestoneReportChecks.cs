namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorMilestoneReportChecks
{
    public static void VisitorMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706634057);
        var items = new List<VisitorMilestone>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(VisitorMilestone.NewVisitorMilestone($"visitor-milestone-{index}", "sample"));
        }
        var report = VisitorMilestoneReport.BuildVisitorMilestoneReport("test", items, clock);
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
        VisitorMilestoneReportCountsEachRecordOnce();
    }
}
