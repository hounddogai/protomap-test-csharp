namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PrimaryAttestationPolicyViewChecks
{
    public static void PrimaryAttestationPolicyViewShowsTheStatusInCapitals()
    {
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("code-959", "name-898");
        var view = PrimaryAttestationPolicyView.ToPrimaryAttestationPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryAttestationPolicyView.ToPrimaryAttestationPolicyViews(new List<PrimaryAttestationPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryAttestationPolicyViewShowsTheStatusInCapitals();
    }
}
