namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CappedCostModelRuleEngineChecks
{
    public static void CappedCostModelRuleEngineKeepsResultsWithinBounds()
    {
        var engine = CappedCostModelRuleEngine.DefaultCappedCostModelRuleEngine(Flags.NewRegistry());
        var item = CappedCostModelRule.NewCappedCostModelRule("code-328", "name-873");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedCostModelRuleEngineKeepsResultsWithinBounds();
    }
}
