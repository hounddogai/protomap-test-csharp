namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class CappedRefreshSlotViewChecks
{
    public static void CappedRefreshSlotViewShowsTheStatusInCapitals()
    {
        var item = CappedRefreshSlot.NewCappedRefreshSlot("code-581", "label-412");
        var view = CappedRefreshSlotView.ToCappedRefreshSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedRefreshSlotView.ToCappedRefreshSlotViews(new List<CappedRefreshSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedRefreshSlotViewShowsTheStatusInCapitals();
    }
}
