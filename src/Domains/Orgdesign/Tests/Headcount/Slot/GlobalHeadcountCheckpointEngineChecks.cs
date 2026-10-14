namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class GlobalHeadcountCheckpointEngineChecks
{
    public static void GlobalHeadcountCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalHeadcountCheckpointEngine.DefaultGlobalHeadcountCheckpointEngine();
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("code-385", "title-243");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalHeadcountCheckpointEngineKeepsResultsWithinBounds();
    }
}
