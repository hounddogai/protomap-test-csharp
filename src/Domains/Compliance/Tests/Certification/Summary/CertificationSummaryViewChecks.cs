namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationSummaryViewChecks
{
    public static void CertificationSummaryViewShowsTheStatusInCapitals()
    {
        var item = CertificationSummary.NewCertificationSummary("code-1", "label-362");
        var view = CertificationSummaryView.ToCertificationSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationSummaryView.ToCertificationSummaryViews(new List<CertificationSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationSummaryViewShowsTheStatusInCapitals();
    }
}
