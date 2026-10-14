namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class LenientFloorPlanAuditViewChecks
{
    public static void LenientFloorPlanAuditViewShowsTheStatusInCapitals()
    {
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("code-814", "name-582");
        var view = LenientFloorPlanAuditView.ToLenientFloorPlanAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientFloorPlanAuditView.ToLenientFloorPlanAuditViews(new List<LenientFloorPlanAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientFloorPlanAuditViewShowsTheStatusInCapitals();
    }
}
