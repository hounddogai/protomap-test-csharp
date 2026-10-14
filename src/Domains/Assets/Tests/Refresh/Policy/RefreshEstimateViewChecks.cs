namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RefreshEstimateViewChecks
{
    public static void RefreshEstimateViewShowsTheStatusInCapitals()
    {
        var item = RefreshEstimate.NewRefreshEstimate("code-367", "name-361");
        var view = RefreshEstimateView.ToRefreshEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshEstimateView.ToRefreshEstimateViews(new List<RefreshEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshEstimateViewShowsTheStatusInCapitals();
    }
}
