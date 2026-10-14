namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PrimaryRefreshVarianceViewChecks
{
    public static void PrimaryRefreshVarianceViewShowsTheStatusInCapitals()
    {
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("code-763", "label-978");
        var view = PrimaryRefreshVarianceView.ToPrimaryRefreshVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryRefreshVarianceView.ToPrimaryRefreshVarianceViews(new List<PrimaryRefreshVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryRefreshVarianceViewShowsTheStatusInCapitals();
    }
}
