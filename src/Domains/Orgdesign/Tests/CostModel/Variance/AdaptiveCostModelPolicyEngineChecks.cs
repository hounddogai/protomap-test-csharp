namespace Acme.Workforce.Orgdesign.Tests.CostModel.Variance;

using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveCostModelPolicyEngineChecks
{
    public static void AdaptiveCostModelPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveCostModelPolicyEngine.DefaultAdaptiveCostModelPolicyEngine();
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("code-769", "label-350");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveCostModelPolicyEngineKeepsResultsWithinBounds();
    }
}
