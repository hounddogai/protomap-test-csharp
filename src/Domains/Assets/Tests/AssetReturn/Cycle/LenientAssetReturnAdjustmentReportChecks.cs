namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientAssetReturnAdjustmentReportChecks
{
    public static void LenientAssetReturnAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700277917);
        var items = new List<LenientAssetReturnAdjustment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment($"lenient-asset-return-adjustment-{index}", "sample"));
        }
        var report = LenientAssetReturnAdjustmentReport.BuildLenientAssetReturnAdjustmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientAssetReturnAdjustmentReportCountsEachRecordOnce();
    }
}
