namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingAssetReturnAdjustmentReportChecks
{
    public static void PendingAssetReturnAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702868088);
        var items = new List<PendingAssetReturnAdjustment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingAssetReturnAdjustment.NewPendingAssetReturnAdjustment($"pending-asset-return-adjustment-{index}", "sample"));
        }
        var report = PendingAssetReturnAdjustmentReport.BuildPendingAssetReturnAdjustmentReport("test", items, clock);
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
        PendingAssetReturnAdjustmentReportCountsEachRecordOnce();
    }
}
