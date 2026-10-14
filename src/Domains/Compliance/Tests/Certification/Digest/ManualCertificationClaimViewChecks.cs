namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ManualCertificationClaimViewChecks
{
    public static void ManualCertificationClaimViewShowsTheStatusInCapitals()
    {
        var item = ManualCertificationClaim.NewManualCertificationClaim("code-283", "name-54");
        var view = ManualCertificationClaimView.ToManualCertificationClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualCertificationClaimView.ToManualCertificationClaimViews(new List<ManualCertificationClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualCertificationClaimViewShowsTheStatusInCapitals();
    }
}
