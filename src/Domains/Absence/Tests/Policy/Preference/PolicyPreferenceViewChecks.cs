namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyPreferenceViewChecks
{
    public static void PolicyPreferenceViewShowsTheStatusInCapitals()
    {
        var item = PolicyPreference.NewPolicyPreference("code-879", "title-426");
        var view = PolicyPreferenceView.ToPolicyPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyPreferenceView.ToPolicyPreferenceViews(new List<PolicyPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyPreferenceViewShowsTheStatusInCapitals();
    }
}
