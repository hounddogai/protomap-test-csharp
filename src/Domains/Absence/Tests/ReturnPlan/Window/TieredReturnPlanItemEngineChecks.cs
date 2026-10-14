namespace Acme.Workforce.Absence.Tests.ReturnPlan.Window;

using Acme.Workforce.Absence.ReturnPlan.Window;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredReturnPlanItemEngineChecks
{
    public static void TieredReturnPlanItemEngineKeepsResultsWithinBounds()
    {
        var engine = TieredReturnPlanItemEngine.DefaultTieredReturnPlanItemEngine(Flags.NewRegistry());
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("code-76", "title-175");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredReturnPlanItemEngineKeepsResultsWithinBounds();
    }
}
