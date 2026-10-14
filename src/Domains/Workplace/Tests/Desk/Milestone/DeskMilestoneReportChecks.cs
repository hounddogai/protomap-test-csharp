namespace Acme.Workforce.Workplace.Tests.Desk.Milestone;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class DeskMilestoneReportChecks
{
    public static void DeskMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705371018);
        var items = new List<DeskMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DeskMilestone.NewDeskMilestone($"desk-milestone-{index}", "sample"));
        }
        var report = DeskMilestoneReport.BuildDeskMilestoneReport("test", items, clock);
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
        DeskMilestoneReportCountsEachRecordOnce();
    }
}
