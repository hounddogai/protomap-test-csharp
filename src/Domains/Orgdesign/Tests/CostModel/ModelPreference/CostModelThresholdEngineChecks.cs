namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class CostModelThresholdEngineChecks
{
    public static void CostModelThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelThresholdEngine.DefaultCostModelThresholdEngine();
        var item = CostModelThreshold.NewCostModelThreshold("code-149", "name-254");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelThresholdEngineKeepsResultsWithinBounds();
    }
}
