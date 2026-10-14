namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalCostModelPreferenceViewChecks
{
    public static void SeasonalCostModelPreferenceViewShowsTheStatusInCapitals()
    {
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("code-88", "name-728");
        var view = SeasonalCostModelPreferenceView.ToSeasonalCostModelPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalCostModelPreferenceView.ToSeasonalCostModelPreferenceViews(new List<SeasonalCostModelPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalCostModelPreferenceViewShowsTheStatusInCapitals();
    }
}
