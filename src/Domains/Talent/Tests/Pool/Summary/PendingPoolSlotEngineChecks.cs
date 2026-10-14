namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingPoolSlotEngineChecks
{
    public static void PendingPoolSlotEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPoolSlotEngine.DefaultPendingPoolSlotEngine();
        var item = PendingPoolSlot.NewPendingPoolSlot("code-917", "label-91");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPoolSlotEngineKeepsResultsWithinBounds();
    }
}
