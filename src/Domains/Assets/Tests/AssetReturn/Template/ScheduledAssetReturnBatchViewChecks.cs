namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAssetReturnBatchViewChecks
{
    public static void ScheduledAssetReturnBatchViewShowsTheStatusInCapitals()
    {
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("code-601", "name-193");
        var view = ScheduledAssetReturnBatchView.ToScheduledAssetReturnBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledAssetReturnBatchView.ToScheduledAssetReturnBatchViews(new List<ScheduledAssetReturnBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledAssetReturnBatchViewShowsTheStatusInCapitals();
    }
}
