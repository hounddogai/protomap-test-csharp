namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgCheckpointEngineChecks
{
    public static void ReorgCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgCheckpointEngine.DefaultReorgCheckpointEngine();
        var item = ReorgCheckpoint.NewReorgCheckpoint("code-880", "name-406");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgCheckpointEngineKeepsResultsWithinBounds();
    }
}
