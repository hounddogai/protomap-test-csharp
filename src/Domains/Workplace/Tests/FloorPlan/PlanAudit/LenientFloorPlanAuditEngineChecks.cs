namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class LenientFloorPlanAuditEngineChecks
{
    public static void LenientFloorPlanAuditEngineKeepsResultsWithinBounds()
    {
        var engine = LenientFloorPlanAuditEngine.DefaultLenientFloorPlanAuditEngine(Flags.NewRegistry());
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("code-814", "name-582");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientFloorPlanAuditEngineKeepsResultsWithinBounds();
    }
}
