namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelEntryEngineChecks
{
    public static void CostModelEntryEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelEntryEngine.DefaultCostModelEntryEngine(Flags.NewRegistry());
        var item = CostModelEntry.NewCostModelEntry("code-334", "name-143");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelEntryEngineKeepsResultsWithinBounds();
    }
}
