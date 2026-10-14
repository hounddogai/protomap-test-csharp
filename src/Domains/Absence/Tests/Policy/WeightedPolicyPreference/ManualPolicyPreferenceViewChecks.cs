namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class ManualPolicyPreferenceViewChecks
{
    public static void ManualPolicyPreferenceViewShowsTheStatusInCapitals()
    {
        var item = ManualPolicyPreference.NewManualPolicyPreference("code-3", "label-400");
        var view = ManualPolicyPreferenceView.ToManualPolicyPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPolicyPreferenceView.ToManualPolicyPreferenceViews(new List<ManualPolicyPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPolicyPreferenceViewShowsTheStatusInCapitals();
    }
}
