namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class StrictReorgEstimateViewChecks
{
    public static void StrictReorgEstimateViewShowsTheStatusInCapitals()
    {
        var item = StrictReorgEstimate.NewStrictReorgEstimate("code-28", "name-85");
        var view = StrictReorgEstimateView.ToStrictReorgEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictReorgEstimateView.ToStrictReorgEstimateViews(new List<StrictReorgEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictReorgEstimateViewShowsTheStatusInCapitals();
    }
}
