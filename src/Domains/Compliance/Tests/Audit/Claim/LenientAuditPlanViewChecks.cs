namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientAuditPlanViewChecks
{
    public static void LenientAuditPlanViewShowsTheStatusInCapitals()
    {
        var item = LenientAuditPlan.NewLenientAuditPlan("code-474", "label-825");
        var view = LenientAuditPlanView.ToLenientAuditPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientAuditPlanView.ToLenientAuditPlanViews(new List<LenientAuditPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientAuditPlanViewShowsTheStatusInCapitals();
    }
}
