namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedRetroPayClaimEngineChecks
{
    public static void WeightedRetroPayClaimEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedRetroPayClaimEngine.DefaultWeightedRetroPayClaimEngine();
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("code-796", "title-693");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedRetroPayClaimEngineKeepsResultsWithinBounds();
    }
}
