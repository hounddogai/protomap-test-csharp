namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantySnapshotEngineChecks
{
    public static void WarrantySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantySnapshotEngine.DefaultWarrantySnapshotEngine(Flags.NewRegistry());
        var item = WarrantySnapshot.NewWarrantySnapshot("code-576", "label-767");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantySnapshotEngineKeepsResultsWithinBounds();
    }
}
