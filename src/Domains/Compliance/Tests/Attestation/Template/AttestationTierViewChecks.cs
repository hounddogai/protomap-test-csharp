namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AttestationTierViewChecks
{
    public static void AttestationTierViewShowsTheStatusInCapitals()
    {
        var item = AttestationTier.NewAttestationTier("code-309", "label-465");
        var view = AttestationTierView.ToAttestationTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationTierView.ToAttestationTierViews(new List<AttestationTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationTierViewShowsTheStatusInCapitals();
    }
}
