namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class StrictPolicyPreferenceViewChecks
{
    public static void StrictPolicyPreferenceViewShowsTheStatusInCapitals()
    {
        var item = StrictPolicyPreference.NewStrictPolicyPreference("code-691", "title-468");
        var view = StrictPolicyPreferenceView.ToStrictPolicyPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPolicyPreferenceView.ToStrictPolicyPreferenceViews(new List<StrictPolicyPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPolicyPreferenceViewShowsTheStatusInCapitals();
    }
}
