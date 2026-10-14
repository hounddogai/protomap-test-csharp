namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRollupViewChecks
{
    public static void RefreshRollupViewShowsTheStatusInCapitals()
    {
        var item = RefreshRollup.NewRefreshRollup("code-161", "label-754");
        var view = RefreshRollupView.ToRefreshRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshRollupView.ToRefreshRollupViews(new List<RefreshRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshRollupViewShowsTheStatusInCapitals();
    }
}
