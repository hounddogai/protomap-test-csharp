namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalExceptionPreferenceViewChecks
{
    public static void RegionalExceptionPreferenceViewShowsTheStatusInCapitals()
    {
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("code-623", "label-904");
        var view = RegionalExceptionPreferenceView.ToRegionalExceptionPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalExceptionPreferenceView.ToRegionalExceptionPreferenceViews(new List<RegionalExceptionPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalExceptionPreferenceViewShowsTheStatusInCapitals();
    }
}
