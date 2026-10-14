namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanAuditViewChecks
{
    public static void FloorPlanAuditViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanAudit.NewFloorPlanAudit("code-510", "title-824");
        var view = FloorPlanAuditView.ToFloorPlanAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanAuditView.ToFloorPlanAuditViews(new List<FloorPlanAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanAuditViewShowsTheStatusInCapitals();
    }
}
