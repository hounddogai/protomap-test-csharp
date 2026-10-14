namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPreferenceViewChecks
{
    public static void AttestationPreferenceViewShowsTheStatusInCapitals()
    {
        var item = AttestationPreference.NewAttestationPreference("code-25", "label-548");
        var view = AttestationPreferenceView.ToAttestationPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationPreferenceView.ToAttestationPreferenceViews(new List<AttestationPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationPreferenceViewShowsTheStatusInCapitals();
    }
}
