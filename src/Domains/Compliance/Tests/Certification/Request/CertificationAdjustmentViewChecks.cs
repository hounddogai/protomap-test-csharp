namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationAdjustmentViewChecks
{
    public static void CertificationAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = CertificationAdjustment.NewCertificationAdjustment("code-331", "label-30");
        var view = CertificationAdjustmentView.ToCertificationAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationAdjustmentView.ToCertificationAdjustmentViews(new List<CertificationAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationAdjustmentViewShowsTheStatusInCapitals();
    }
}
