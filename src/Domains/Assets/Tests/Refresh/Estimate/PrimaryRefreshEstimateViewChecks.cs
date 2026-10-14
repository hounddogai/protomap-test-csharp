namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshEstimateViewChecks
{
    public static void PrimaryRefreshEstimateViewShowsTheStatusInCapitals()
    {
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("code-525", "label-623");
        var view = PrimaryRefreshEstimateView.ToPrimaryRefreshEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryRefreshEstimateView.ToPrimaryRefreshEstimateViews(new List<PrimaryRefreshEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryRefreshEstimateViewShowsTheStatusInCapitals();
    }
}
