namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedReorgCheckpointEngineChecks
{
    public static void BatchedReorgCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedReorgCheckpointEngine.DefaultBatchedReorgCheckpointEngine(Flags.NewRegistry());
        var item = BatchedReorgCheckpoint.NewBatchedReorgCheckpoint("code-172", "title-821");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedReorgCheckpointEngineKeepsResultsWithinBounds();
    }
}
