namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualAssetReturnBatchReportChecks
{
    public static void ManualAssetReturnBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700343600);
        var items = new List<ManualAssetReturnBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualAssetReturnBatch.NewManualAssetReturnBatch($"manual-asset-return-batch-{index}", "sample"));
        }
        var report = ManualAssetReturnBatchReport.BuildManualAssetReturnBatchReport("test", items, clock);
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
        ManualAssetReturnBatchReportCountsEachRecordOnce();
    }
}
