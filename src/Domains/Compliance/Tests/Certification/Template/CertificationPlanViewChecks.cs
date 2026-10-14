namespace Acme.Workforce.Compliance.Tests.Certification.Template;

using Acme.Workforce.Compliance.Certification.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CertificationPlanViewChecks
{
    public static void CertificationPlanViewShowsTheStatusInCapitals()
    {
        var item = CertificationPlan.NewCertificationPlan("code-465", "name-549");
        var view = CertificationPlanView.ToCertificationPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationPlanView.ToCertificationPlanViews(new List<CertificationPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationPlanViewShowsTheStatusInCapitals();
    }
}
