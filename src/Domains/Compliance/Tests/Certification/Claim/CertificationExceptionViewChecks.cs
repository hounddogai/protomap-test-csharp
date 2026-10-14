namespace Acme.Workforce.Compliance.Tests.Certification.Claim;

using Acme.Workforce.Compliance.Certification.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class CertificationExceptionViewChecks
{
    public static void CertificationExceptionViewShowsTheStatusInCapitals()
    {
        var item = CertificationException.NewCertificationException("code-695", "label-880");
        var view = CertificationExceptionView.ToCertificationExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationExceptionView.ToCertificationExceptionViews(new List<CertificationException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationExceptionViewShowsTheStatusInCapitals();
    }
}
