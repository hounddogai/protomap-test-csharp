namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPolicyViewChecks
{
    public static void AttestationPolicyViewShowsTheStatusInCapitals()
    {
        var item = AttestationPolicy.NewAttestationPolicy("code-435", "name-281");
        var view = AttestationPolicyView.ToAttestationPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationPolicyView.ToAttestationPolicyViews(new List<AttestationPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationPolicyViewShowsTheStatusInCapitals();
    }
}
