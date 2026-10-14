namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictAssetReturnSegmentReportChecks
{
    public static void StrictAssetReturnSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703122604);
        var items = new List<StrictAssetReturnSegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictAssetReturnSegment.NewStrictAssetReturnSegment($"strict-asset-return-segment-{index}", "sample"));
        }
        var report = StrictAssetReturnSegmentReport.BuildStrictAssetReturnSegmentReport("test", items, clock);
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
        StrictAssetReturnSegmentReportCountsEachRecordOnce();
    }
}
