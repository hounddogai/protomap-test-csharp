namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantySegmentReportChecks
{
    public static void WarrantySegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705786714);
        var items = new List<WarrantySegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WarrantySegment.NewWarrantySegment($"warranty-segment-{index}", "sample"));
        }
        var report = WarrantySegmentReport.BuildWarrantySegmentReport("test", items, clock);
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
        WarrantySegmentReportCountsEachRecordOnce();
    }
}
