namespace Acme.Workforce.Assets.Tests.Refresh.Quota;

using Acme.Workforce.Assets.Refresh.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RefreshSlotViewChecks
{
    public static void RefreshSlotViewShowsTheStatusInCapitals()
    {
        var item = RefreshSlot.NewRefreshSlot("code-817", "name-217");
        var view = RefreshSlotView.ToRefreshSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshSlotView.ToRefreshSlotViews(new List<RefreshSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshSlotViewShowsTheStatusInCapitals();
    }
}
