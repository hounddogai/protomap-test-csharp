namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class LegacyOccupancyRecordReportChecks
{
    public static void LegacyOccupancyRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702453639);
        var items = new List<LegacyOccupancyRecord>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyOccupancyRecord.NewLegacyOccupancyRecord($"legacy-occupancy-record-{index}", "sample"));
        }
        var report = LegacyOccupancyRecordReport.BuildLegacyOccupancyRecordReport("test", items, clock);
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
        LegacyOccupancyRecordReportCountsEachRecordOnce();
    }
}
