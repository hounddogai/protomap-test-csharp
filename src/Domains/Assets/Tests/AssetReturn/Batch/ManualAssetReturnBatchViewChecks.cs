namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualAssetReturnBatchViewChecks
{
    public static void ManualAssetReturnBatchViewShowsTheStatusInCapitals()
    {
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("code-959", "label-370");
        var view = ManualAssetReturnBatchView.ToManualAssetReturnBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualAssetReturnBatchView.ToManualAssetReturnBatchViews(new List<ManualAssetReturnBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualAssetReturnBatchViewShowsTheStatusInCapitals();
    }
}
