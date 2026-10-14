namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingReorgEntryEngineChecks
{
    public static void PendingReorgEntryEngineKeepsResultsWithinBounds()
    {
        var engine = PendingReorgEntryEngine.DefaultPendingReorgEntryEngine();
        var item = PendingReorgEntry.NewPendingReorgEntry("code-206", "title-841");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingReorgEntryEngineKeepsResultsWithinBounds();
    }
}
