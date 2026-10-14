namespace Acme.Workforce.Payrollops.Tests.Deduction.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DeductionSnapshotReportChecks
{
    public static void DeductionSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701314601);
        var items = new List<DeductionSnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DeductionSnapshot.NewDeductionSnapshot($"deduction-snapshot-{index}", "sample"));
        }
        var report = DeductionSnapshotReport.BuildDeductionSnapshotReport("test", items, clock);
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
        DeductionSnapshotReportCountsEachRecordOnce();
    }
}
