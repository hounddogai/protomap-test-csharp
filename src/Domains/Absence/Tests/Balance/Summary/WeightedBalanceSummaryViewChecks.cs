namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedBalanceSummaryViewChecks
{
    public static void WeightedBalanceSummaryViewShowsTheStatusInCapitals()
    {
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("code-590", "label-598");
        var view = WeightedBalanceSummaryView.ToWeightedBalanceSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedBalanceSummaryView.ToWeightedBalanceSummaryViews(new List<WeightedBalanceSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedBalanceSummaryViewShowsTheStatusInCapitals();
    }
}
