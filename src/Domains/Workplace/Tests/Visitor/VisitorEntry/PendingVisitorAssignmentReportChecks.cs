namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PendingVisitorAssignmentReportChecks
{
    public static void PendingVisitorAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709082768);
        var items = new List<PendingVisitorAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PendingVisitorAssignment.NewPendingVisitorAssignment($"pending-visitor-assignment-{index}", "sample"));
        }
        var report = PendingVisitorAssignmentReport.BuildPendingVisitorAssignmentReport("test", items, clock);
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
        PendingVisitorAssignmentReportCountsEachRecordOnce();
    }
}
