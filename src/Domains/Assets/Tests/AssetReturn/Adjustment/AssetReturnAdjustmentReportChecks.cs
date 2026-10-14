namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnAdjustmentReportChecks
{
    public static void AssetReturnAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708338775);
        var items = new List<AssetReturnAdjustment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnAdjustment.NewAssetReturnAdjustment($"asset-return-adjustment-{index}", "sample"));
        }
        var report = AssetReturnAdjustmentReport.BuildAssetReturnAdjustmentReport("test", items, clock);
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
        AssetReturnAdjustmentReportCountsEachRecordOnce();
    }
}
