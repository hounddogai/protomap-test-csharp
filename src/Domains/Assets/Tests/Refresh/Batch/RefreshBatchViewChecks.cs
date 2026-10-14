namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshBatchViewChecks
{
    public static void RefreshBatchViewShowsTheStatusInCapitals()
    {
        var item = RefreshBatch.NewRefreshBatch("code-366", "label-821");
        var view = RefreshBatchView.ToRefreshBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshBatchView.ToRefreshBatchViews(new List<RefreshBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshBatchViewShowsTheStatusInCapitals();
    }
}
