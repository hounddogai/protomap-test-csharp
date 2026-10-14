namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceBaselineViewChecks
{
    public static void AdaptiveBalanceBaselineViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("code-331", "title-179");
        var view = AdaptiveBalanceBaselineView.ToAdaptiveBalanceBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveBalanceBaselineView.ToAdaptiveBalanceBaselineViews(new List<AdaptiveBalanceBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveBalanceBaselineViewShowsTheStatusInCapitals();
    }
}
