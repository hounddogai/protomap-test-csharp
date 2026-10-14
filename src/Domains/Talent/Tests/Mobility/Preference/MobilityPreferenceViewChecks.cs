namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class MobilityPreferenceViewChecks
{
    public static void MobilityPreferenceViewShowsTheStatusInCapitals()
    {
        var item = MobilityPreference.NewMobilityPreference("code-771", "title-41");
        var view = MobilityPreferenceView.ToMobilityPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityPreferenceView.ToMobilityPreferenceViews(new List<MobilityPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityPreferenceViewShowsTheStatusInCapitals();
    }
}
