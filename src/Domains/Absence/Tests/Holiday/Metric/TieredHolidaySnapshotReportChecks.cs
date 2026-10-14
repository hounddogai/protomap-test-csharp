namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredHolidaySnapshotReportChecks
{
    public static void TieredHolidaySnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703210108);
        var items = new List<TieredHolidaySnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredHolidaySnapshot.NewTieredHolidaySnapshot($"tiered-holiday-snapshot-{index}", "sample"));
        }
        var report = TieredHolidaySnapshotReport.BuildTieredHolidaySnapshotReport("test", items, clock);
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
        TieredHolidaySnapshotReportCountsEachRecordOnce();
    }
}
