namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationMetricViewChecks
{
    public static void CertificationMetricViewShowsTheStatusInCapitals()
    {
        var item = CertificationMetric.NewCertificationMetric("code-898", "label-534");
        var view = CertificationMetricView.ToCertificationMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationMetricView.ToCertificationMetricViews(new List<CertificationMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationMetricViewShowsTheStatusInCapitals();
    }
}
