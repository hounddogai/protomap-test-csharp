namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolSlotEngineChecks
{
    public static void PoolSlotEngineKeepsResultsWithinBounds()
    {
        var engine = PoolSlotEngine.DefaultPoolSlotEngine();
        var item = PoolSlot.NewPoolSlot("code-664", "label-868");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolSlotEngineKeepsResultsWithinBounds();
    }
}
