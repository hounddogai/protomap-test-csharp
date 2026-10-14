namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshAdjustmentViewChecks
{
    public static void RefreshAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = RefreshAdjustment.NewRefreshAdjustment("code-269", "label-790");
        var view = RefreshAdjustmentView.ToRefreshAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshAdjustmentView.ToRefreshAdjustmentViews(new List<RefreshAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshAdjustmentViewShowsTheStatusInCapitals();
    }
}
