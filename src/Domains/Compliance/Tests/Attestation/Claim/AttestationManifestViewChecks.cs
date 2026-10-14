namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationManifestViewChecks
{
    public static void AttestationManifestViewShowsTheStatusInCapitals()
    {
        var item = AttestationManifest.NewAttestationManifest("code-230", "label-850");
        var view = AttestationManifestView.ToAttestationManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationManifestView.ToAttestationManifestViews(new List<AttestationManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationManifestViewShowsTheStatusInCapitals();
    }
}
