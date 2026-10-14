namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanDraftEngineChecks
{
    public static void FloorPlanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanDraftEngine.DefaultFloorPlanDraftEngine();
        var item = FloorPlanDraft.NewFloorPlanDraft("code-862", "name-343");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanDraftEngineKeepsResultsWithinBounds();
    }
}
