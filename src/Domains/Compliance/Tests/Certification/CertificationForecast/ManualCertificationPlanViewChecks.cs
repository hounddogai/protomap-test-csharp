namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationPlanViewChecks
{
    public static void ManualCertificationPlanViewShowsTheStatusInCapitals()
    {
        var item = ManualCertificationPlan.NewManualCertificationPlan("code-689", "name-20");
        var view = ManualCertificationPlanView.ToManualCertificationPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualCertificationPlanView.ToManualCertificationPlanViews(new List<ManualCertificationPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualCertificationPlanViewShowsTheStatusInCapitals();
    }
}
