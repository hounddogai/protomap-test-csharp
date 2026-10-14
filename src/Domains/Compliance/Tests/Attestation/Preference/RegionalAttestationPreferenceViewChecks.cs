namespace Acme.Workforce.Compliance.Tests.Attestation.Preference;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalAttestationPreferenceViewChecks
{
    public static void RegionalAttestationPreferenceViewShowsTheStatusInCapitals()
    {
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("code-194", "title-656");
        var view = RegionalAttestationPreferenceView.ToRegionalAttestationPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalAttestationPreferenceView.ToRegionalAttestationPreferenceViews(new List<RegionalAttestationPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalAttestationPreferenceViewShowsTheStatusInCapitals();
    }
}
