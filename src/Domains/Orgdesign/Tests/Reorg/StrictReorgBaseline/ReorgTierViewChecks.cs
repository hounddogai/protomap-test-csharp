namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class ReorgTierViewChecks
{
    public static void ReorgTierViewShowsTheStatusInCapitals()
    {
        var item = ReorgTier.NewReorgTier("code-827", "title-418");
        var view = ReorgTierView.ToReorgTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgTierView.ToReorgTierViews(new List<ReorgTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgTierViewShowsTheStatusInCapitals();
    }
}
