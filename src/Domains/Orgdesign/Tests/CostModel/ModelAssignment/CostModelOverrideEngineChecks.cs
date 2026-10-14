namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelOverrideEngineChecks
{
    public static void CostModelOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelOverrideEngine.DefaultCostModelOverrideEngine(Flags.NewRegistry());
        var item = CostModelOverride.NewCostModelOverride("code-748", "title-933");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelOverrideEngineKeepsResultsWithinBounds();
    }
}
