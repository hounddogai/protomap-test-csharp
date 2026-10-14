namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredOccupancyRecordReportChecks
{
    public static void TieredOccupancyRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709892232);
        var items = new List<TieredOccupancyRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredOccupancyRecord.NewTieredOccupancyRecord($"tiered-occupancy-record-{index}", "sample"));
        }
        var report = TieredOccupancyRecordReport.BuildTieredOccupancyRecordReport("test", items, clock);
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
        TieredOccupancyRecordReportCountsEachRecordOnce();
    }
}
