namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CertificationBundleViewChecks
{
    public static void CertificationBundleViewShowsTheStatusInCapitals()
    {
        var item = CertificationBundle.NewCertificationBundle("code-348", "label-340");
        var view = CertificationBundleView.ToCertificationBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationBundleView.ToCertificationBundleViews(new List<CertificationBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationBundleViewShowsTheStatusInCapitals();
    }
}
