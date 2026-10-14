namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SuccessionPlanItemEngineChecks
{
    public static void SuccessionPlanItemEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanItemEngine.DefaultSuccessionPlanItemEngine();
        var item = SuccessionPlanItem.NewSuccessionPlanItem("code-790", "title-634");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanItemEngineKeepsResultsWithinBounds();
    }
}
