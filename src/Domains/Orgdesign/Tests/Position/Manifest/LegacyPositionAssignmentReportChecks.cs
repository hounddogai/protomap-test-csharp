namespace Acme.Workforce.Orgdesign.Tests.Position.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LegacyPositionAssignmentReportChecks
{
    public static void LegacyPositionAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707039019);
        var items = new List<LegacyPositionAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyPositionAssignment.NewLegacyPositionAssignment($"legacy-position-assignment-{index}", "sample"));
        }
        var report = LegacyPositionAssignmentReport.BuildLegacyPositionAssignmentReport("test", items, clock);
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
        LegacyPositionAssignmentReportCountsEachRecordOnce();
    }
}
