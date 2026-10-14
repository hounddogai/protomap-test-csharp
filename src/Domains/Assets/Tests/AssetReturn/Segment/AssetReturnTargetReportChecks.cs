namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AssetReturnTargetReportChecks
{
    public static void AssetReturnTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704427945);
        var items = new List<AssetReturnTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AssetReturnTarget.NewAssetReturnTarget($"asset-return-target-{index}", "sample"));
        }
        var report = AssetReturnTargetReport.BuildAssetReturnTargetReport("test", items, clock);
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
        AssetReturnTargetReportCountsEachRecordOnce();
    }
}
