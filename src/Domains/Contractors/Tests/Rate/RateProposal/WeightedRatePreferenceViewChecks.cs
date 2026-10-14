namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class WeightedRatePreferenceViewChecks
{
    public static void WeightedRatePreferenceViewShowsTheStatusInCapitals()
    {
        var item = WeightedRatePreference.NewWeightedRatePreference("code-521", "title-820");
        var view = WeightedRatePreferenceView.ToWeightedRatePreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedRatePreferenceView.ToWeightedRatePreferenceViews(new List<WeightedRatePreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedRatePreferenceViewShowsTheStatusInCapitals();
    }
}
