namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class FallbackHolidaySnapshotReportChecks
{
    public static void FallbackHolidaySnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703863311);
        var items = new List<FallbackHolidaySnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackHolidaySnapshot.NewFallbackHolidaySnapshot($"fallback-holiday-snapshot-{index}", "sample"));
        }
        var report = FallbackHolidaySnapshotReport.BuildFallbackHolidaySnapshotReport("test", items, clock);
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
        FallbackHolidaySnapshotReportCountsEachRecordOnce();
    }
}
