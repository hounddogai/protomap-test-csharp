namespace Acme.Workforce.Orgdesign.Tests.Reorg.Baseline;

using Acme.Workforce.Orgdesign.Reorg.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ReorgBaselineViewChecks
{
    public static void ReorgBaselineViewShowsTheStatusInCapitals()
    {
        var item = ReorgBaseline.NewReorgBaseline("code-977", "title-547");
        var view = ReorgBaselineView.ToReorgBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgBaselineView.ToReorgBaselineViews(new List<ReorgBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgBaselineViewShowsTheStatusInCapitals();
    }
}
