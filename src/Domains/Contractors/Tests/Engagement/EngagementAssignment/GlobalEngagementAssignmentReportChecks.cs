namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class GlobalEngagementAssignmentReportChecks
{
    public static void GlobalEngagementAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702157607);
        var items = new List<GlobalEngagementAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalEngagementAssignment.NewGlobalEngagementAssignment($"global-engagement-assignment-{index}", "sample"));
        }
        var report = GlobalEngagementAssignmentReport.BuildGlobalEngagementAssignmentReport("test", items, clock);
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
        GlobalEngagementAssignmentReportCountsEachRecordOnce();
    }
}
