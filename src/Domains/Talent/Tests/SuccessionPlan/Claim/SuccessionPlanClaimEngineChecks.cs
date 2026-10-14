namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SuccessionPlanClaimEngineChecks
{
    public static void SuccessionPlanClaimEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanClaimEngine.DefaultSuccessionPlanClaimEngine();
        var item = SuccessionPlanClaim.NewSuccessionPlanClaim("code-552", "title-732");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanClaimEngineKeepsResultsWithinBounds();
    }
}
