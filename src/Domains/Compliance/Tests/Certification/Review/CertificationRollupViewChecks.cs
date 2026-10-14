namespace Acme.Workforce.Compliance.Tests.Certification.Review;

using Acme.Workforce.Compliance.Certification.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CertificationRollupViewChecks
{
    public static void CertificationRollupViewShowsTheStatusInCapitals()
    {
        var item = CertificationRollup.NewCertificationRollup("code-634", "label-832");
        var view = CertificationRollupView.ToCertificationRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationRollupView.ToCertificationRollupViews(new List<CertificationRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationRollupViewShowsTheStatusInCapitals();
    }
}
