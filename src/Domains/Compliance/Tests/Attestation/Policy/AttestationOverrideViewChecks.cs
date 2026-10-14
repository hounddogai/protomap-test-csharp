namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationOverrideViewChecks
{
    public static void AttestationOverrideViewShowsTheStatusInCapitals()
    {
        var item = AttestationOverride.NewAttestationOverride("code-413", "label-847");
        var view = AttestationOverrideView.ToAttestationOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationOverrideView.ToAttestationOverrideViews(new List<AttestationOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationOverrideViewShowsTheStatusInCapitals();
    }
}
