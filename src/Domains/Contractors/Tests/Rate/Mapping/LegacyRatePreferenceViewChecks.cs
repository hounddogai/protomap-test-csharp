namespace Acme.Workforce.Contractors.Tests.Rate.Mapping;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyRatePreferenceViewChecks
{
    public static void LegacyRatePreferenceViewShowsTheStatusInCapitals()
    {
        var item = LegacyRatePreference.NewLegacyRatePreference("code-596", "title-600");
        var view = LegacyRatePreferenceView.ToLegacyRatePreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyRatePreferenceView.ToLegacyRatePreferenceViews(new List<LegacyRatePreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyRatePreferenceViewShowsTheStatusInCapitals();
    }
}
