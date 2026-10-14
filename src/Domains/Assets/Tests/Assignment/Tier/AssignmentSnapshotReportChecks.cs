namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentSnapshotReportChecks
{
    public static void AssignmentSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705768934);
        var items = new List<AssignmentSnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssignmentSnapshot.NewAssignmentSnapshot($"assignment-snapshot-{index}", "sample"));
        }
        var report = AssignmentSnapshotReport.BuildAssignmentSnapshotReport("test", items, clock);
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
        AssignmentSnapshotReportCountsEachRecordOnce();
    }
}
