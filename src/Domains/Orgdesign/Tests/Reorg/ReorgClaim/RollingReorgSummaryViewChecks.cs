namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgClaim;

using Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>Checks of reorg claim code, run by the project's runner.</summary>
public static class RollingReorgSummaryViewChecks
{
    public static void RollingReorgSummaryViewShowsTheStatusInCapitals()
    {
        var item = RollingReorgSummary.NewRollingReorgSummary("code-752", "name-175");
        var view = RollingReorgSummaryView.ToRollingReorgSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingReorgSummaryView.ToRollingReorgSummaryViews(new List<RollingReorgSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingReorgSummaryViewShowsTheStatusInCapitals();
    }
}
