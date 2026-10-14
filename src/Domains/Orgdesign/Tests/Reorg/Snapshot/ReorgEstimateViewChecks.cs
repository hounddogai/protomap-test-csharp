namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgEstimateViewChecks
{
    public static void ReorgEstimateViewShowsTheStatusInCapitals()
    {
        var item = ReorgEstimate.NewReorgEstimate("code-145", "label-26");
        var view = ReorgEstimateView.ToReorgEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgEstimateView.ToReorgEstimateViews(new List<ReorgEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgEstimateViewShowsTheStatusInCapitals();
    }
}
