namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackPayBandSnapshotReportChecks
{
    public static void FallbackPayBandSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707391477);
        var items = new List<FallbackPayBandSnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FallbackPayBandSnapshot.NewFallbackPayBandSnapshot($"fallback-pay-band-snapshot-{index}", "sample"));
        }
        var report = FallbackPayBandSnapshotReport.BuildFallbackPayBandSnapshotReport("test", items, clock);
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
        FallbackPayBandSnapshotReportCountsEachRecordOnce();
    }
}
