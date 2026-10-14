namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSnapshotReportChecks
{
    public static void AdjustmentSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702913965);
        var items = new List<AdjustmentSnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdjustmentSnapshot.NewAdjustmentSnapshot($"adjustment-snapshot-{index}", "sample"));
        }
        var report = AdjustmentSnapshotReport.BuildAdjustmentSnapshotReport("test", items, clock);
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
        AdjustmentSnapshotReportCountsEachRecordOnce();
    }
}
