namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionAssignmentReportChecks
{
    public static void PositionAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709897826);
        var items = new List<PositionAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PositionAssignment.NewPositionAssignment($"position-assignment-{index}", "sample"));
        }
        var report = PositionAssignmentReport.BuildPositionAssignmentReport("test", items, clock);
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
        PositionAssignmentReportCountsEachRecordOnce();
    }
}
