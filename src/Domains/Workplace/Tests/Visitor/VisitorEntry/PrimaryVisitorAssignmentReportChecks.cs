namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PrimaryVisitorAssignmentReportChecks
{
    public static void PrimaryVisitorAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704235117);
        var items = new List<PrimaryVisitorAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryVisitorAssignment.NewPrimaryVisitorAssignment($"primary-visitor-assignment-{index}", "sample"));
        }
        var report = PrimaryVisitorAssignmentReport.BuildPrimaryVisitorAssignmentReport("test", items, clock);
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
        PrimaryVisitorAssignmentReportCountsEachRecordOnce();
    }
}
