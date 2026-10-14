namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPayBandSnapshotReportChecks
{
    public static void ScheduledPayBandSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707736313);
        var items = new List<ScheduledPayBandSnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot($"scheduled-pay-band-snapshot-{index}", "sample"));
        }
        var report = ScheduledPayBandSnapshotReport.BuildScheduledPayBandSnapshotReport("test", items, clock);
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
        ScheduledPayBandSnapshotReportCountsEachRecordOnce();
    }
}
