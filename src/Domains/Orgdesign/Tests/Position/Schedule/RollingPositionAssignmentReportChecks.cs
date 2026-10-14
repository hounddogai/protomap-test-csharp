namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingPositionAssignmentReportChecks
{
    public static void RollingPositionAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708017784);
        var items = new List<RollingPositionAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RollingPositionAssignment.NewRollingPositionAssignment($"rolling-position-assignment-{index}", "sample"));
        }
        var report = RollingPositionAssignmentReport.BuildRollingPositionAssignmentReport("test", items, clock);
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
        RollingPositionAssignmentReportCountsEachRecordOnce();
    }
}
