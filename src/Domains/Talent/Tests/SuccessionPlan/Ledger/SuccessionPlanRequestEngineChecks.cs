namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRequestEngineChecks
{
    public static void SuccessionPlanRequestEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanRequestEngine.DefaultSuccessionPlanRequestEngine();
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("code-835", "label-658");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanRequestEngineKeepsResultsWithinBounds();
    }
}
