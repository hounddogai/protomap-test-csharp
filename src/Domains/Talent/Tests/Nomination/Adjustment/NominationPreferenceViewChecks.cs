namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class NominationPreferenceViewChecks
{
    public static void NominationPreferenceViewShowsTheStatusInCapitals()
    {
        var item = NominationPreference.NewNominationPreference("code-805", "name-754");
        var view = NominationPreferenceView.ToNominationPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationPreferenceView.ToNominationPreferenceViews(new List<NominationPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationPreferenceViewShowsTheStatusInCapitals();
    }
}
