namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WeightedSuccessionPlanRequestEngineChecks
{
    public static void WeightedSuccessionPlanRequestEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedSuccessionPlanRequestEngine.DefaultWeightedSuccessionPlanRequestEngine();
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("code-246", "label-390");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedSuccessionPlanRequestEngineKeepsResultsWithinBounds();
    }
}
