namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentSegmentReportChecks
{
    public static void SeasonalAdjustmentSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709928417);
        var items = new List<SeasonalAdjustmentSegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment($"seasonal-adjustment-segment-{index}", "sample"));
        }
        var report = SeasonalAdjustmentSegmentReport.BuildSeasonalAdjustmentSegmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentSegmentReportCountsEachRecordOnce();
    }
}
