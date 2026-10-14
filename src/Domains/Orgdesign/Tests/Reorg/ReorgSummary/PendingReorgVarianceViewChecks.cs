namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class PendingReorgVarianceViewChecks
{
    public static void PendingReorgVarianceViewShowsTheStatusInCapitals()
    {
        var item = PendingReorgVariance.NewPendingReorgVariance("code-951", "name-387");
        var view = PendingReorgVarianceView.ToPendingReorgVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingReorgVarianceView.ToPendingReorgVarianceViews(new List<PendingReorgVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingReorgVarianceViewShowsTheStatusInCapitals();
    }
}
