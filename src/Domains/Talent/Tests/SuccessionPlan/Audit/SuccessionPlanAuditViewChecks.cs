namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAuditViewChecks
{
    public static void SuccessionPlanAuditViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("code-166", "name-870");
        var view = SuccessionPlanAuditView.ToSuccessionPlanAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanAuditView.ToSuccessionPlanAuditViews(new List<SuccessionPlanAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanAuditViewShowsTheStatusInCapitals();
    }
}
