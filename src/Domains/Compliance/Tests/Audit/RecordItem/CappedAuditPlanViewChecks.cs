namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedAuditPlanViewChecks
{
    public static void CappedAuditPlanViewShowsTheStatusInCapitals()
    {
        var item = CappedAuditPlan.NewCappedAuditPlan("code-43", "label-39");
        var view = CappedAuditPlanView.ToCappedAuditPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAuditPlanView.ToCappedAuditPlanViews(new List<CappedAuditPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAuditPlanViewShowsTheStatusInCapitals();
    }
}
