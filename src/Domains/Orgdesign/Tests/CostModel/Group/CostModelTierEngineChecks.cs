namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CostModelTierEngineChecks
{
    public static void CostModelTierEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelTierEngine.DefaultCostModelTierEngine();
        var item = CostModelTier.NewCostModelTier("code-995", "name-304");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelTierEngineKeepsResultsWithinBounds();
    }
}
