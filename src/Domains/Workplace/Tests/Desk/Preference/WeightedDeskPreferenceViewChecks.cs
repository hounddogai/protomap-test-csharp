namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedDeskPreferenceViewChecks
{
    public static void WeightedDeskPreferenceViewShowsTheStatusInCapitals()
    {
        var item = WeightedDeskPreference.NewWeightedDeskPreference("code-217", "label-343");
        var view = WeightedDeskPreferenceView.ToWeightedDeskPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedDeskPreferenceView.ToWeightedDeskPreferenceViews(new List<WeightedDeskPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedDeskPreferenceViewShowsTheStatusInCapitals();
    }
}
