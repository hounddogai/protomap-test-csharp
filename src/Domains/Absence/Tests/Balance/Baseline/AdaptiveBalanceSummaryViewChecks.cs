namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceSummaryViewChecks
{
    public static void AdaptiveBalanceSummaryViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("code-456", "label-781");
        var view = AdaptiveBalanceSummaryView.ToAdaptiveBalanceSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveBalanceSummaryView.ToAdaptiveBalanceSummaryViews(new List<AdaptiveBalanceSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveBalanceSummaryViewShowsTheStatusInCapitals();
    }
}
