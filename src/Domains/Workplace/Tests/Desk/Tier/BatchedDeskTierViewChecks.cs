namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedDeskTierViewChecks
{
    public static void BatchedDeskTierViewShowsTheStatusInCapitals()
    {
        var item = BatchedDeskTier.NewBatchedDeskTier("code-381", "label-115");
        var view = BatchedDeskTierView.ToBatchedDeskTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedDeskTierView.ToBatchedDeskTierViews(new List<BatchedDeskTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedDeskTierViewShowsTheStatusInCapitals();
    }
}
