namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DefaultVisitorAssignmentReportChecks
{
    public static void DefaultVisitorAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706432986);
        var items = new List<DefaultVisitorAssignment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultVisitorAssignment.NewDefaultVisitorAssignment($"default-visitor-assignment-{index}", "sample"));
        }
        var report = DefaultVisitorAssignmentReport.BuildDefaultVisitorAssignmentReport("test", items, clock);
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
        DefaultVisitorAssignmentReportCountsEachRecordOnce();
    }
}
