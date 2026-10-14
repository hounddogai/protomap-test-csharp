namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationEstimateViewChecks
{
    public static void CertificationEstimateViewShowsTheStatusInCapitals()
    {
        var item = CertificationEstimate.NewCertificationEstimate("code-844", "title-663");
        var view = CertificationEstimateView.ToCertificationEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationEstimateView.ToCertificationEstimateViews(new List<CertificationEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationEstimateViewShowsTheStatusInCapitals();
    }
}
