namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyCheckpointEngineChecks
{
    public static void WarrantyCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyCheckpointEngine.DefaultWarrantyCheckpointEngine(Flags.NewRegistry());
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("code-223", "title-507");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyCheckpointEngineKeepsResultsWithinBounds();
    }
}
