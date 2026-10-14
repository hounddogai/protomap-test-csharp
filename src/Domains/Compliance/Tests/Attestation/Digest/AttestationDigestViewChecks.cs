namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationDigestViewChecks
{
    public static void AttestationDigestViewShowsTheStatusInCapitals()
    {
        var item = AttestationDigest.NewAttestationDigest("code-429", "title-860");
        var view = AttestationDigestView.ToAttestationDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationDigestView.ToAttestationDigestViews(new List<AttestationDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationDigestViewShowsTheStatusInCapitals();
    }
}
