namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentTargetReportChecks
{
    public static void AssignmentTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709669724);
        var items = new List<AssignmentTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AssignmentTarget.NewAssignmentTarget($"assignment-target-{index}", "sample"));
        }
        var report = AssignmentTargetReport.BuildAssignmentTargetReport("test", items, clock);
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
        AssignmentTargetReportCountsEachRecordOnce();
    }
}
