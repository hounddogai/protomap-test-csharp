namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackCostModelRollupEngineChecks
{
    public static void FallbackCostModelRollupEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackCostModelRollupEngine.DefaultFallbackCostModelRollupEngine(Flags.NewRegistry());
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("code-649", "name-914");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackCostModelRollupEngineKeepsResultsWithinBounds();
    }
}
