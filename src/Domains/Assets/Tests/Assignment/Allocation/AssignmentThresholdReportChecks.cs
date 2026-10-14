namespace Acme.Workforce.Assets.Tests.Assignment.Allocation;

using Acme.Workforce.Assets.Assignment.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AssignmentThresholdReportChecks
{
    public static void AssignmentThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702995993);
        var items = new List<AssignmentThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AssignmentThreshold.NewAssignmentThreshold($"assignment-threshold-{index}", "sample"));
        }
        var report = AssignmentThresholdReport.BuildAssignmentThresholdReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AssignmentThresholdReportCountsEachRecordOnce();
    }
}
