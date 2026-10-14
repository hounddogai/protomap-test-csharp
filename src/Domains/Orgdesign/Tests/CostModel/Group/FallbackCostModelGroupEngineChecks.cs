namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FallbackCostModelGroupEngineChecks
{
    public static void FallbackCostModelGroupEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackCostModelGroupEngine.DefaultFallbackCostModelGroupEngine();
        var item = FallbackCostModelGroup.NewFallbackCostModelGroup("code-755", "title-767");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackCostModelGroupEngineKeepsResultsWithinBounds();
    }
}
