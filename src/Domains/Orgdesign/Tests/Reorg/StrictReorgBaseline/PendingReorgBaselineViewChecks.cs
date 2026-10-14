namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class PendingReorgBaselineViewChecks
{
    public static void PendingReorgBaselineViewShowsTheStatusInCapitals()
    {
        var item = PendingReorgBaseline.NewPendingReorgBaseline("code-565", "label-283");
        var view = PendingReorgBaselineView.ToPendingReorgBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingReorgBaselineView.ToPendingReorgBaselineViews(new List<PendingReorgBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingReorgBaselineViewShowsTheStatusInCapitals();
    }
}
